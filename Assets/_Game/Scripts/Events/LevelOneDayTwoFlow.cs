using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using BullyingGame.UI;

namespace BullyingGame.LevelOne
{
    public class LevelOneDayTwoFlow : MonoBehaviour
    {
        [Header("Day two route")]
        [SerializeField] private LevelOneDirector director;
        [SerializeField] private NavMeshAgent bima;
        [SerializeField] private GameObject[] cinematicStandIns;
        [SerializeField] private GameObject memet, raka;
        [Header("L01-C01")]
        [SerializeField] private string incidentScene;
        [SerializeField] private string timelineName = "Timeline Canteen";
        bool playing, incidentZoneOccupied;
        Animator bimaAnimator;
        static readonly int Speed = Animator.StringToHash("Speed");
        string lastStep, placedStep;

        void Awake()
        {
            var animator = bima.GetComponentInChildren<Animator>();
            if (animator != null && animator.parameters.Any(parameter =>
                    parameter.nameHash == Speed && parameter.type == AnimatorControllerParameterType.Float))
                bimaAnimator = animator;
        }

        public void RestoreBima(LevelOneSaveData progress)
        {
            if (progress.hasBimaPosition && NavMesh.SamplePosition(progress.bimaPosition, out var hit, 2, NavMesh.AllAreas))
                bima.Warp(hit.position);
        }

        public void CaptureBima(LevelOneSaveData progress)
        {
            if (!bima.isOnNavMesh) return;
            progress.bimaPosition = bima.transform.position;
            progress.hasBimaPosition = true;
        }

        public bool CanInteract(LevelOnePoint point)
        {
            switch (director.Step.id)
            {
                case "incident":
                    return !playing && IsBimaNear(point.transform, 3f);
                case "return":
                    return IsBimaNear(point.transform, 2.6f);
                case "report":
                    return IsBimaNear(point.transform, 4.5f);
                default:
                    return true;
            }
        }

        bool IsBimaNear(Transform point, float distance) =>
            (bima.transform.position - point.position).sqrMagnitude <= distance * distance;

        void Update()
        {
            if (!director.Ready) return;
            var step = director.Step?.id;
            var target = director.Target;
            if (step != lastStep)
            {
                lastStep = step;
                memet.SetActive(step == "words");
                raka.SetActive(step == "words");
            }
            if (director.IsModal || target == null)
            {
                if (bima.isOnNavMesh) bima.isStopped = true;
                if (bimaAnimator != null) bimaAnimator.SetFloat(Speed, 0);
                GroundMarker.ClearTarget();
                Minimap.ClearMissionTarget();
                return;
            }
            GroundMarker.SetTarget(target.transform.position);
            Minimap.SetMissionTarget(target.transform.position);
            if (step == "clue") director.PresentActionCard();

            if (!bima.isOnNavMesh) return;
            if (step == "incident")
            {
                bima.isStopped = Vector3.Distance(bima.transform.position, director.Player.transform.position) > 10f;
                if (!bima.isStopped) bima.SetDestination(target.transform.position);
                bool ready = CanInteract(target) && director.IsReachable(target);
                if (ready && !incidentZoneOccupied) StartIncident();
                incidentZoneOccupied = ready;
            }
            else if (step == "report")
            {
                bima.isStopped = false;
                Vector3 playerPosition = director.Player.transform.position;
                if (NavMesh.SamplePosition(playerPosition, out var position, 2, NavMesh.AllAreas))
                    bima.SetDestination(position.position);
            }
            else
            {
                bima.isStopped = true;
                if (placedStep != step && (step == "clue" || step == "return" || step == "words"))
                {
                    if (NavMesh.SamplePosition(target.transform.position + Vector3.right * 1.4f, out var position, 2, NavMesh.AllAreas) &&
                        bima.Warp(position.position))
                        placedStep = step;
                }
            }
            if (bimaAnimator != null) bimaAnimator.SetFloat(Speed, bima.velocity.magnitude, .15f, Time.deltaTime);
        }

        public void StartIncident()
        {
            if (playing || director.IsModal || director.Step?.id != "incident" ||
                !CanInteract(director.Target) || !director.IsReachable(director.Target)) return;
            playing = true;
            director.BeginIncident();
            StartCoroutine(PlayIncident());
        }

        IEnumerator PlayIncident()
        {
            if (!Application.CanStreamedLevelBeLoaded(incidentScene))
            {
                Debug.LogError("L01-C01 is missing from the build scene list: " + incidentScene);
                Finish(false);
                yield break;
            }
            if (bima.isOnNavMesh) bima.isStopped = true;
            var renderers = cinematicStandIns.SelectMany(actor => actor.GetComponentsInChildren<Renderer>())
                .Where(renderer => renderer.enabled).ToArray();
            foreach (var renderer in renderers) renderer.enabled = false;

            var gameplayCamera = UnityEngine.Camera.main;
            var gameplayBrain = gameplayCamera != null ? gameplayCamera.GetComponent<CinemachineBrain>() : null;
            var gameplayListener = gameplayCamera != null ? gameplayCamera.GetComponent<AudioListener>() : null;
            bool brainWasEnabled = gameplayBrain != null && gameplayBrain.enabled;
            bool listenerWasEnabled = gameplayListener != null && gameplayListener.enabled;
            if (gameplayCamera != null) gameplayCamera.enabled = false;
            if (brainWasEnabled) gameplayBrain.enabled = false;
            if (listenerWasEnabled) gameplayListener.enabled = false;

            AsyncOperation load = null;
            try { load = SceneManager.LoadSceneAsync(incidentScene, LoadSceneMode.Additive); }
            catch (System.Exception exception) { Debug.LogException(exception); }
            if (load != null) yield return load;
            var scene = SceneManager.GetSceneByPath(incidentScene);
            var timeline = scene.IsValid() && scene.isLoaded
                ? scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<PlayableDirector>())
                    .FirstOrDefault(candidate => candidate.name == timelineName)
                : null;
            bool completed = false;
            var brain = timeline?.playableAsset?.outputs
                .Where(output => output.outputTargetType == typeof(CinemachineBrain))
                .Select(output => timeline.GetGenericBinding(output.sourceObject) as CinemachineBrain)
                .FirstOrDefault();
            if (brain != null && brain.gameObject.scene == scene && brain.isActiveAndEnabled &&
                brain.TryGetComponent<UnityEngine.Camera>(out var cutsceneCamera) && cutsceneCamera.isActiveAndEnabled &&
                timeline.duration > 0 && !double.IsInfinity(timeline.duration))
            {
                timeline.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
                timeline.extrapolationMode = DirectorWrapMode.None;
                double duration = timeline.duration;
                bool stopped = false;
                void OnStopped(PlayableDirector source) => stopped = source == timeline;
                timeline.stopped += OnStopped;
                timeline.Play();
                double start = Time.unscaledTimeAsDouble;
                double deadline = start + duration + 5;
                double observedTime = 0;
                while (!stopped && Time.unscaledTimeAsDouble < deadline)
                {
                    observedTime = System.Math.Max(observedTime, timeline.time);
                    yield return null;
                }
                completed = stopped && (observedTime >= duration - .35 ||
                    Time.unscaledTimeAsDouble - start >= duration - .35);
                timeline.stopped -= OnStopped;
                timeline.Stop();
            }
            if (!completed) Debug.LogError("L01-C01 did not complete its authored Timeline.");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            if (gameplayCamera != null) gameplayCamera.enabled = true;
            if (brainWasEnabled && gameplayBrain != null) gameplayBrain.enabled = true;
            if (listenerWasEnabled && gameplayListener != null) gameplayListener.enabled = true;
            foreach (var renderer in renderers) if (renderer != null) renderer.enabled = true;
            Finish(completed);
        }

        void Finish(bool completed)
        {
            playing = false;
            director.FinishIncident(completed);
        }
    }
}

using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using BullyingGame.UI;
using BullyingGame.Interaction;

namespace BullyingGame.LevelOne
{
    public class LevelOneDayTwoFlow : MonoBehaviour
    {
        [Header("Day two route")]
        [SerializeField] private LevelOneDirector director;
        [SerializeField] private NavMeshAgent bima;
        [SerializeField] private SwingDoor classroomExit;
        [SerializeField] private SwingDoor reportClassroomDoor;
        [SerializeField] private LevelOneIncidentProps incidentProps;
        [SerializeField] private GameObject[] cinematicStandIns;
        [Header("L01-C01")]
        [SerializeField] private string incidentScene;
        [SerializeField] private AreaSekolahGraphicsController gameplayEnvironment;
        [SerializeField] private string timelineName = "Timeline Canteen";
        bool playing, incidentZoneOccupied, propsInitialized, pickingUp;
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
                    return !playing && (incidentProps == null || director.Progress.bimaPackedLunch) &&
                        IsBimaNear(point.transform, 3f);
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
            if (!propsInitialized)
            {
                propsInitialized = true;
                if (incidentProps != null && step == "incident")
                {
                    // Older checkpoints can already be on the way to the canteen.
                    if ((bima.transform.position - incidentProps.PickupPosition).sqrMagnitude > 8f * 8f)
                        director.Progress.bimaPackedLunch = true;
                    if (director.Progress.bimaPackedLunch) incidentProps.Carry();
                }
                else if (incidentProps != null && step != "greet")
                    incidentProps.AfterIncident(director.Progress.bottleTaken);
            }
            if (step != lastStep)
            {
                if (lastStep == null && step == "greet")
                {
                    var bell = Resources.Load<AudioClip>("Voice/bell");
                    if (bell != null) AudioSource.PlayClipAtPoint(bell, bima.transform.position + Vector3.up, .8f);
                }
                lastStep = step;
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
                if (incidentProps != null && !director.Progress.bimaPackedLunch) ApproachLunch();
                else
                {
                    bima.isStopped = Vector3.Distance(bima.transform.position, director.Player.transform.position) > 10f;
                    if (!bima.isStopped) WalkTo(target.transform.position);
                    bool ready = CanInteract(target) && director.IsReachable(target);
                    if (ready && !incidentZoneOccupied) StartIncident();
                    incidentZoneOccupied = ready;
                }
            }
            else if (step == "report")
            {
                bima.isStopped = false;
                Vector3 playerPosition = director.Player.transform.position;
                WalkTo(playerPosition);
                OpenNearbyDoor(reportClassroomDoor);
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

        void ApproachLunch()
        {
            if (pickingUp) return;
            var delta = Vector3.ProjectOnPlane(incidentProps.PickupPosition - bima.transform.position, Vector3.up);
            float reach = Mathf.Max(bima.stoppingDistance + .1f, .28f);
            if (delta.sqrMagnitude > reach * reach)
            {
                bima.isStopped = false;
                WalkTo(incidentProps.PickupPosition);
                return;
            }
            bima.isStopped = true;
            bima.ResetPath();
            StartCoroutine(PackLunch());
        }

        IEnumerator PackLunch()
        {
            pickingUp = true;
            yield return incidentProps.PickUp();
            director.Progress.bimaPackedLunch = true;
            pickingUp = false;
            director.Save();
        }

        void WalkTo(Vector3 destination)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = bima.agentTypeID, areaMask = bima.areaMask };
            if (!NavMesh.SamplePosition(destination, out var position, 2f, filter)) return;
            bima.SetDestination(position.position);

            OpenNearbyDoor(classroomExit);
        }

        void OpenNearbyDoor(SwingDoor door)
        {
            if (door != null && door.CanInteract() &&
                (door.DoorwayPosition - bima.transform.position - Vector3.up).sqrMagnitude < 2.5f * 2.5f)
                door.Open();
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
            var hub = gameObject.scene;
            var school = gameplayEnvironment != null ? gameplayEnvironment.gameObject : null;
            var schoolRenderers = school != null ? school.GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled && !renderer.forceRenderingOff).ToArray() : System.Array.Empty<Renderer>();
            var schoolLights = school != null ? school.GetComponentsInChildren<Light>()
                .Where(light => light.enabled).ToArray() : System.Array.Empty<Light>();
            var schoolVolumes = school != null ? school.GetComponentsInChildren<UnityEngine.Rendering.Volume>()
                .Where(volume => volume.enabled).ToArray() : System.Array.Empty<UnityEngine.Rendering.Volume>();
            var schoolProbes = school != null ? school.GetComponentsInChildren<ReflectionProbe>()
                .Where(probe => probe.enabled).ToArray() : System.Array.Empty<ReflectionProbe>();
            var schoolColliders = school != null ? school.GetComponentsInChildren<Collider>()
                .Where(collider => collider.enabled).ToArray() : System.Array.Empty<Collider>();
            // Keep the gameplay NavMeshSurface loaded while hiding the overlapping campus.
            foreach (var renderer in schoolRenderers) renderer.forceRenderingOff = true;
            foreach (var light in schoolLights) light.enabled = false;
            foreach (var volume in schoolVolumes) volume.enabled = false;
            foreach (var probe in schoolProbes) probe.enabled = false;
            foreach (var collider in schoolColliders) collider.enabled = false;

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
            if (scene.IsValid() && scene.isLoaded && SceneManager.SetActiveScene(scene))
            {
                var cutsceneEnvironment = scene.GetRootGameObjects()
                    .Select(root => root.GetComponent<AreaSekolahGraphicsController>())
                    .FirstOrDefault(graphics => graphics != null && graphics.gameObject.activeInHierarchy);
                cutsceneEnvironment?.ApplyGraphicsMode();
            }
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
                incidentProps?.PlayTimeline(timeline);
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
            if (incidentProps != null)
            {
                if (completed) incidentProps.AfterIncident(director.Progress.bottleTaken);
                else incidentProps.Carry();
            }
            if (!completed) Debug.LogError("L01-C01 did not complete its authored Timeline.");
            if (hub.IsValid() && hub.isLoaded) SceneManager.SetActiveScene(hub);
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            foreach (var renderer in schoolRenderers) if (renderer != null) renderer.forceRenderingOff = false;
            foreach (var light in schoolLights) if (light != null) light.enabled = true;
            foreach (var volume in schoolVolumes) if (volume != null) volume.enabled = true;
            foreach (var probe in schoolProbes) if (probe != null) probe.enabled = true;
            foreach (var collider in schoolColliders) if (collider != null) collider.enabled = true;
            if (gameplayEnvironment != null) gameplayEnvironment.ApplyGraphicsMode();
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

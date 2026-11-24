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
        [SerializeField] private Animator[] corridorStudents = new Animator[0];
        static readonly string[] Questions = {
            "Sahrul, kapan ejekan termasuk perundungan?",
            "Vivi, apa yang bisa dirasakan teman yang sering dirundung?",
            "Nando, kalau kita melihatnya, apa yang bisa kita lakukan?"
        };
        static readonly string[] Answers = {
            "Sahrul: Kalau seseorang terus mengejek teman yang sulit membela diri, itu perundungan. Misalnya, satu kelompok terus memanggil dia dengan julukan yang menyakitkan.",
            "Vivi: Dia bisa merasa takut masuk kelas atau sulit konsentrasi. Kadang dia diam karena takut diejek lagi. Kita bisa menemaninya dan mendengarkan ceritanya.",
            "Nando: Teman yang dirundung bisa takut masuk kelas. Kalau yang melihat ikut tertawa, teman lain bisa mengira ejekan itu wajar. Kita bisa menemani teman itu dan mengajaknya bicara dengan guru yang dia percaya."
        };
        [SerializeField] private Vector3 interviewWaitPoint = new Vector3(12.45f, .1f, 20.9f);
        private float escortSpeed;
        private bool reachedDoor;
        private CinemachineCamera conversationCamera;
        private BullyingGame.Prologue.PrologueDialogueShot currentShot;

        internal void BeginShot(Transform actor, Transform other = null)
        {
            EndShot();
            if (actor == null) return;
            if (conversationCamera == null)
            {
                var rig = new GameObject("Level01 Dialogue Camera");
                rig.transform.SetParent(transform, false);
                conversationCamera = rig.AddComponent<CinemachineCamera>();
            }
            currentShot = actor.GetComponent<BullyingGame.Prologue.PrologueDialogueShot>();
            if (currentShot == null) currentShot = actor.gameObject.AddComponent<BullyingGame.Prologue.PrologueDialogueShot>();
            currentShot.EnterLevelShot(other != null ? other : director.Player.transform, conversationCamera);
        }

        internal void BeginStoryShot()
        {
            var teacher = director.Step.id == "report" ? GameObject.Find("IbuGuru_Kelas") : null;
            if (director.Step.id == "words") { BeginShot(incidentProps.Pursuer.transform); return; }
            BeginShot(teacher != null ? teacher.transform : bima.transform);
        }

        internal void EndShot()
        {
            if (currentShot != null) currentShot.Exit();
            currentShot = null;
        }

        bool InterviewPending => corridorStudents.Length >= 3 && director.Progress.corridorInterviews < 2;
        public Animator InterviewStudent => director.Ready && director.Step?.id == "incident" &&
            reachedDoor && InterviewPending
            ? corridorStudents[director.Progress.corridorInterviews == 0 ? 0 : 2] : null;
        public string StudentName => director.Progress.corridorInterviews == 0 ? "Sahrul" : "Nando";
        public bool CanAsk => InterviewStudent != null &&
            Vector3.Distance(director.Player.transform.position, InterviewStudent.transform.position) <= 2.3f;
        public string CanteenObjective => InterviewStudent != null
            ? $"Tanya {StudentName} tentang perundungan ({director.Progress.corridorInterviews}/2). Bima menunggu di koridor."
            : "Ikuti Bima menuju kantin.";
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
            escortSpeed = bima.speed;
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
                    return !InterviewPending && !playing && (incidentProps == null || director.Progress.bimaPackedLunch) &&
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
            if (step != lastStep && (step == "bottle" || step == "return" || step == "report"))
            {
                incidentProps.PlaceLunch(director.Point("P5").transform.position, director.Progress.lunchTaken);
                if (director.Progress.returned) incidentProps.ReturnLunch();
            }
            var pursuer = incidentProps.Pursuer;
            if (director.Progress.bottleTaken && !director.Progress.returned && pursuer.isActiveAndEnabled && pursuer.isOnNavMesh)
            {
                pursuer.isStopped = director.IsModal && !chaseFirstStep;
                if (!director.IsModal)
                {
                    pursuer.speed = director.Progress.lunchTaken ? 5.8f : 3.8f;
                    pursuer.stoppingDistance = .15f;
                    pursuer.SetDestination(director.Player.transform.position);
                    if (Vector3.Distance(pursuer.transform.position, director.Player.transform.position) < .85f)
                    {
                        pursuer.ResetPath();
                        director.Caught();
                        return;
                    }
                }
            }
            else if (director.Progress.returned && pursuer.isActiveAndEnabled && pursuer.isOnNavMesh)
                pursuer.isStopped = true;
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
                else if (InterviewPending)
                {
                    float reach = Mathf.Max(.55f, bima.stoppingDistance + .15f);
                    reachedDoor = Vector3.ProjectOnPlane(interviewWaitPoint - bima.transform.position, Vector3.up).sqrMagnitude <= reach * reach;
                    if (!reachedDoor)
                    {
                        bima.speed = Mathf.Max(escortSpeed, 4.2f);
                        bima.isStopped = false;
                        WalkTo(interviewWaitPoint);
                        if (bimaAnimator != null) bimaAnimator.SetFloat(Speed, bima.velocity.magnitude, .15f, Time.deltaTime);
                        return;
                    }
                    bima.speed = escortSpeed;
                    bima.isStopped = true;
                    if (!director.Progress.waitingForInterviews)
                    {
                        director.Progress.waitingForInterviews = true;
                        director.Save();
                    }
                    Face(bima.transform, director.Player.transform.position);
                    GroundMarker.SetTarget(InterviewStudent.transform.position);
                    Minimap.SetMissionTarget(InterviewStudent.transform.position);
                }
                else
                {
                    bima.isStopped = Vector3.Distance(bima.transform.position, director.Player.transform.position) > 10f;
                    if (!bima.isStopped) WalkTo(target.transform.position);
                    else Face(bima.transform, director.Player.transform.position);
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
                if (step == "greet") Face(bima.transform, director.Player.transform.position);
                if (placedStep != "pillar" && (step == "clue" || step == "words" || step == "bottle" || step == "return"))
                {
                    if (NavMesh.SamplePosition(director.Point("Bench").transform.position + Vector3.right * 1.4f, out var position, 2, NavMesh.AllAreas) &&
                        bima.Warp(position.position))
                        placedStep = "pillar";
                }
            }
            if (bimaAnimator != null) bimaAnimator.SetFloat(Speed, bima.velocity.magnitude, .15f, Time.deltaTime);
        }

        static void Face(Transform actor, Vector3 point)
        {
            var direction = Vector3.ProjectOnPlane(point - actor.position, Vector3.up);
            if (direction.sqrMagnitude > .01f)
                actor.rotation = Quaternion.RotateTowards(actor.rotation, Quaternion.LookRotation(direction), 270f * Time.deltaTime);
        }

        public void AskStudent()
        {
            if (!CanAsk || director.IsModal) return;
            int i = director.Progress.corridorInterviews == 0 ? 0 : 2;
            var student = corridorStudents[i];
            var player = director.Player.transform.position;
            student.transform.LookAt(new Vector3(player.x, student.transform.position.y, player.z));
            BeginShot(student.transform);
            director.CorridorExchange(Questions[i], Answers[i],
                () => student.CrossFadeInFixedTime("Base Layer.Talking", .15f),
                () => {
                    student.CrossFadeInFixedTime("Base Layer.Idle", .15f);
                    director.Progress.corridorInterviews++;
                    director.Save();
                });
        }

        internal void TakeLunch() => incidentProps.PlaceLunch(director.Point("P5").transform.position, true);

        internal void ResetSearch()
        {
            incidentProps.AfterIncident(false);
            incidentProps.PlaceLunch(director.Point("P5").transform.position, false);
        }

        private bool chaseFirstStep;

        internal IEnumerator ChaseIntro(LevelOneView view, System.Action resume)
        {
            view.Caption("Raka", "Met!");
            yield return new WaitForSecondsRealtime(.6f);
            var pursuer = incidentProps.Pursuer;
            BeginShot(pursuer.transform, incidentProps.Raka);
            view.Caption("Raka", "Met, kalau Bima cerita ke guru gimana?");
            yield return new WaitForSecondsRealtime(2.7f);
            // Let the audience see Memet turn towards Aji before cutting back.
            chaseFirstStep = true;
            pursuer.speed = 3.8f;
            pursuer.isStopped = false;
            pursuer.SetDestination(director.Player.transform.position);
            yield return new WaitForSecondsRealtime(.35f);
            chaseFirstStep = false;
            EndShot();
            resume();
            view.ChaseCall();
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

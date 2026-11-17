using System;
using System.Collections.Generic;
using System.Linq;
using BullyingGame.Core;
using BullyingGame.Interaction;
using BullyingGame.Player;
using BullyingGame.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace BullyingGame.Prologue
{
    public sealed class PrologueFlow : MonoBehaviour
    {
        [Serializable]
        public class Territory
        {
            public string name;
            public Transform arrival;
            [Min(1)] public float radius = 3f;
        }

        private enum Stage { Movement, SpeakToBima, FollowBima, SelectCanteen, GoToCanteen, SelectClass, GoToClass,
            Teacher, Classmates, Loading }

        [SerializeField] private bool introductionScene;
        [SerializeField] private Transform introductionSpawn;
        [SerializeField] private PlayerInputHandler player;
        [SerializeField] private PrologueHud hud;
        [SerializeField] private PrologueDialogueActor bima;
        [SerializeField] private PrologueDialogueActor teacher;
        [SerializeField] private PrologueDialogueActor[] classmates;
        [SerializeField, Min(1)] private int requiredClassmates = 8;
        [SerializeField] private Territory[] territories;
        [SerializeField] private Transform canteenReveal;
        [SerializeField] private NavMeshAgent guide;
        [SerializeField] private SwingDoor classroomDoor;
        [SerializeField] private float talkRange = 3f;

        private static readonly HashSet<int> Visited = new HashSet<int>();
        private readonly HashSet<PrologueDialogueActor> introduced = new HashSet<PrologueDialogueActor>();
        private GameInputActions input;
        private Stage stage;
        private PrologueDialogueActor conversation;
        private PrologueDialogueActor nearby;
        private bool moved, jumped, sprinted;
        private bool mapOpen;
        private Vector3 startPosition;
        private Transform guideGoal;
        private NavMeshPath gpsPath, guidePath;
        private InteractionDetector doors;
        private float nextGpsUpdate, nextGuideAttempt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetVisits() => Visited.Clear();

        private void Awake()
        {
            if (!introductionScene) Visited.Clear();
            else if (introductionSpawn != null)
            {
                var controller = player.GetComponent<CharacterController>();
                controller.enabled = false;
                player.transform.SetPositionAndRotation(introductionSpawn.position, introductionSpawn.rotation);
                controller.enabled = true;
            }
            startPosition = player.transform.position;
            doors = player.GetComponent<InteractionDetector>();
            input = new GameInputActions();
            input.Movement.Enable();

            if (bima != null) bima.SetVisible(false);
            if (teacher != null) teacher.SetVisible(false);
            foreach (var actor in classmates) if (actor != null) actor.SetVisible(false);
        }

        private void Start()
        {
            hud.Initialize(this, territories, player.transform);
            GameStateManager.Instance?.SetState(GameState.Playing);
            player.SetCursorLock(true);
            SetStage(introductionScene ? Stage.Classmates : Stage.Movement);
        }

        private void OnDestroy()
        {
            if (input == null) return;
            input.Movement.Disable();
            input.Dispose();
        }

        private void Update()
        {
            if (stage == Stage.Loading) return;
            bool interact = input.Movement.Interact.WasPressedThisFrame();

            if (conversation != null)
            {
                if (interact && !conversation.Advance()) FinishConversation();
                return;
            }

            if (stage != Stage.Movement && stage != Stage.SpeakToBima &&
                (Keyboard.current?.mKey.wasPressedThisFrame == true ||
                 (mapOpen && Keyboard.current?.escapeKey.wasPressedThisFrame == true)))
            {
                ShowMap(!mapOpen);
                return;
            }
            if (mapOpen) return;

            switch (stage)
            {
                case Stage.Movement:
                    UpdateMovementLesson();
                    break;
                case Stage.FollowBima:
                    if (IsNear(canteenReveal, 6f)) SetStage(Stage.SelectCanteen);
                    break;
                case Stage.GoToCanteen:
                    if (Arrived(0))
                    {
                        Visit(0);
                        SetStage(Stage.SelectClass);
                    }
                    break;
                case Stage.GoToClass:
                    if (Arrived(1))
                    {
                        Visit(1);
                        SetStage(Stage.Teacher);
                    }
                    break;
            }

            if (stage == Stage.Teacher || stage == Stage.Classmates)
            {
                for (int i = 2; i < territories.Length; i++)
                    if (!Visited.Contains(i) && Arrived(i)) Visit(i);
            }

            UpdateGuide();
            UpdateGps();
            nearby = FindNearby();
            IInteractable target = nearby != null ? nearby : doors?.FindNearbyDoor();
            hud.SetPrompt(target != null ? "E  " + target.GetPromptText() : "");
            if (interact) target?.Interact(player.gameObject);
        }

        private void UpdateMovementLesson()
        {
            bool moving = input.Movement.Move.ReadValue<Vector2>().sqrMagnitude > .1f;
            if (moving && FlatDistance(player.transform.position, startPosition) > 1.25f) moved = true;
            if (input.Movement.Jump.WasPressedThisFrame()) jumped = true;
            if (moving && input.Movement.Sprint.IsPressed()) sprinted = true;
            hud.SetTutorial(moved, jumped, sprinted, false);
            if (moved && jumped && sprinted) SetStage(Stage.SpeakToBima);
        }

        public bool CanTalk(PrologueDialogueActor actor) => !mapOpen && conversation == null &&
            ((stage == Stage.SpeakToBima && actor == bima) ||
             (stage == Stage.Teacher && actor == teacher) ||
             (stage == Stage.Classmates && classmates.Contains(actor) && !introduced.Contains(actor)));

        internal void TryTalk(PrologueDialogueActor actor, GameObject interactor)
        {
            if (interactor != player.gameObject || !CanTalk(actor) || !CanReach(actor) || !actor.Begin()) return;
            conversation = actor;
            LockMovement(true);
            hud.SetPrompt("E  Lanjut");
            hud.SetTutorial(false, false, false, true);
        }

        private void FinishConversation()
        {
            PrologueDialogueActor actor = conversation;
            conversation = null;
            LockMovement(false);
            if (stage == Stage.SpeakToBima)
                SetStage(Stage.FollowBima);
            else if (stage == Stage.Teacher)
                StartCutscene(PrologueCinematics.Sequence.MasukKelas);
            else if (stage == Stage.Classmates && introduced.Add(actor))
            {
                if (introduced.Count >= requiredClassmates) StartCutscene(PrologueCinematics.Sequence.TanyaSikka);
                else SetStage(Stage.Classmates);
            }
        }

        private void StartCutscene(PrologueCinematics.Sequence sequence)
        {
            if (PrologueCinematics.Load(sequence))
            {
                SetStage(Stage.Loading);
                LockMovement(true);
            }
            else Debug.LogError("Daftarkan Cutscene perkenalan kelas di Build Settings.", this);
        }

        private PrologueDialogueActor FindNearby()
        {
            if (stage == Stage.SpeakToBima) return CanReach(bima) ? bima : null;
            if (stage == Stage.Teacher) return CanReach(teacher) ? teacher : null;
            if (stage != Stage.Classmates) return null;
            PrologueDialogueActor closest = null;
            float distance = float.PositiveInfinity;
            foreach (var actor in classmates)
            {
                if (actor == null || !actor.CanInteract() || !CanReach(actor)) continue;
                float current = FlatDistance(player.transform.position, actor.transform.position);
                if (current >= distance) continue;
                closest = actor;
                distance = current;
            }
            return closest;
        }

        private bool CanReach(PrologueDialogueActor actor)
        {
            if (actor == null || !actor.isActiveAndEnabled || !IsNear(actor.transform, talkRange)) return false;
            Vector3 from = player.transform.position + Vector3.up * 1.25f;
            Vector3 to = actor.transform.position + Vector3.up * 1.25f;
            Vector3 ray = to - from;
            foreach (var hit in Physics.RaycastAll(from, ray.normalized, ray.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(player.transform) || hit.transform.IsChildOf(actor.transform)) continue;
                return false;
            }
            return true;
        }

        private void SetStage(Stage next)
        {
            stage = next;
            Transform previousGoal = guideGoal;
            guideGoal = next switch
            {
                Stage.FollowBima or Stage.SelectCanteen => canteenReveal,
                Stage.GoToCanteen or Stage.SelectClass => territories[0].arrival,
                Stage.GoToClass or Stage.Teacher => territories[1].arrival,
                _ => null
            };
            if (guideGoal != previousGoal && guide != null && guide.isOnNavMesh)
            {
                guide.ResetPath();
                nextGuideAttempt = 0f;
            }
            Transform target = next switch
            {
                Stage.SpeakToBima => bima.transform,
                Stage.FollowBima => bima.transform,
                Stage.GoToCanteen => territories[0].arrival,
                Stage.GoToClass => territories[1].arrival,
                Stage.Teacher => teacher.transform,
                _ => null
            };

            hud.SetMapAvailable(next is not (Stage.Movement or Stage.SpeakToBima or Stage.Loading));
            hud.SetObjective(next switch
            {
                Stage.Movement => "",
                Stage.SpeakToBima => "Bicara dengan Bima",
                Stage.FollowBima => "Ikuti Bima ke arah Kantin",
                Stage.SelectCanteen => "Buka GPS [M] dan pilih Kantin",
                Stage.GoToCanteen => "Pergi ke Kantin",
                Stage.SelectClass => "Buka GPS [M] dan pilih Kelas 8A",
                Stage.GoToClass => "Pergi ke Kelas 8A",
                Stage.Teacher => "Temui Bu Guru",
                Stage.Classmates => $"Berkenalan dengan teman kelas  {introduced.Count}/{requiredClassmates}",
                _ => ""
            }, target, next is Stage.GoToCanteen or Stage.GoToClass);

            if (next == Stage.Movement || next == Stage.SpeakToBima)
                hud.SetTutorial(moved, jumped, sprinted, false);
            nextGpsUpdate = 0f;

            if (next is Stage.FollowBima or Stage.GoToCanteen or Stage.GoToClass)
            {
                var point = next == Stage.FollowBima ? canteenReveal : guideGoal;
                GroundMarker.SetTarget(point.position);
            }
            else GroundMarker.ClearTarget();
        }

        private void UpdateGps()
        {
            if (stage is not (Stage.GoToCanteen or Stage.GoToClass) || Time.time < nextGpsUpdate) return;
            nextGpsUpdate = Time.time + .25f;
            gpsPath ??= new NavMeshPath();
            Vector3 playerPosition = player.transform.position;
            Vector3 destination = guideGoal.position;
            var filter = new NavMeshQueryFilter { agentTypeID = guide.agentTypeID, areaMask = guide.areaMask };
            if (NavMesh.SamplePosition(playerPosition, out var start, 2f, filter) &&
                NavMesh.SamplePosition(destination, out var end, 2f, filter) &&
                NavMesh.CalculatePath(start.position, end.position, filter, gpsPath) &&
                gpsPath.status == NavMeshPathStatus.PathComplete)
            {
                foreach (var corner in gpsPath.corners)
                {
                    if (FlatDistance(playerPosition, corner) < 1.5f) continue;
                    hud.SetRouteHint(corner);
                    return;
                }
            }
            hud.SetRouteHint(destination);
        }

        private void UpdateGuide()
        {
            if (classroomDoor != null && stage is Stage.GoToClass or Stage.Teacher)
            {
                Vector3 doorway = classroomDoor.DoorwayPosition;
                if (FlatDistance(player.transform.position, doorway) < 4f ||
                    (guide != null && FlatDistance(guide.transform.position, doorway) < 4f))
                    classroomDoor.Open();
            }

            if (guide == null || !guide.isOnNavMesh) return;
            bool paused = guideGoal == null || FlatDistance(player.transform.position, guide.transform.position) > 12f;
            guide.isStopped = paused;
            if (paused || guide.pathPending || guide.hasPath || Time.time < nextGuideAttempt) return;
            if (FlatDistance(guide.transform.position, guideGoal.position) <= guide.stoppingDistance) return;

            nextGuideAttempt = Time.time + .35f;
            guidePath ??= new NavMeshPath();
            var filter = new NavMeshQueryFilter { agentTypeID = guide.agentTypeID, areaMask = guide.areaMask };
            if (NavMesh.SamplePosition(guideGoal.position, out var hit, 1.2f, filter) &&
                guide.CalculatePath(hit.position, guidePath) && guidePath.status == NavMeshPathStatus.PathComplete)
                guide.SetPath(guidePath);
        }

        internal void CloseMap() { if (mapOpen) ShowMap(false); }

        private void ShowMap(bool visible)
        {
            mapOpen = visible;
            LockMovement(visible);
            player.SetCursorLock(!visible);
            if (guide != null && guide.isOnNavMesh) guide.isStopped = visible;
            hud.ShowMap(visible);
        }

        internal bool IsVisited(int index) => Visited.Contains(index);
        internal bool CanSelect(int index) =>
            Visited.Contains(index) || (index == 0 && stage == Stage.SelectCanteen) ||
            (index == 1 && stage == Stage.SelectClass);

        internal void SelectTerritory(int index)
        {
            if (!mapOpen || !CanSelect(index)) return;
            if (index == 0 && stage == Stage.SelectCanteen)
                SetStage(Stage.GoToCanteen);
            else if (index == 1 && stage == Stage.SelectClass)
                SetStage(Stage.GoToClass);
            else return;
            ShowMap(false);
        }

        private void Visit(int index)
        {
            if (!Visited.Add(index)) return;
            string room = territories[index].name;
            Save.SaveManager.Instance?.SetStoryFlag("prologue.visited/" + room);
            hud.ShowBadge(room, Visited.Count, territories.Length);
        }

        private bool Arrived(int index) => IsNear(territories[index].arrival, territories[index].radius);

        private bool IsNear(Transform point, float radius)
        {
            Vector3 position = player.transform.position;
            return Mathf.Abs(position.y - point.position.y) < 1.8f && FlatDistance(position, point.position) <= radius;
        }

        private static float FlatDistance(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;

        private void LockMovement(bool locked)
        {
            player.enabled = !locked;
            player.GetComponent<PlayerMovement>().enabled = !locked;
            if (stage != Stage.Loading)
                GameStateManager.Instance?.SetState(locked ? (mapOpen ? GameState.Quest : GameState.Dialogue) : GameState.Playing);
        }
    }
}

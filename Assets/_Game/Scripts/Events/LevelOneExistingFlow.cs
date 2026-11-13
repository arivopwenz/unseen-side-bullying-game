using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Core;
using BullyingGame.Events;
using BullyingGame.NPC;
using BullyingGame.UI;

namespace BullyingGame.LevelOne
{
    // Original guide, ambush, zones and Timeline own presentation; PRD owns card/item progress.
    [DefaultExecutionOrder(-50)]
    public class LevelOneExistingFlow : MonoBehaviour
    {
        public static LevelOneExistingFlow Current { get; private set; }
        [Header("Original Level01 hierarchy")]
        [SerializeField] private LevelOneDirector quests;
        [SerializeField] private GuideController guide;
        [SerializeField] private AmbushTrigger ambush;
        [SerializeField] private BimaConversationDriver conversation;
        [SerializeField] private Transform classroomReturn, canteenReturn, gardenBench;
        [SerializeField] private LevelOnePoint[] bimaPoints;
        [SerializeField] private string classroomScene = "Cutscene perkenalan kelas";
        [SerializeField] private Behaviour legacyObjective;
        bool initialized, movingToBench;
        int previousStep = -1;
        public GameObject Player => quests != null ? quests.Player : null;
        public bool CanStartAmbush => initialized && !quests.ExternalDone("corridor") && !quests.IsModal;
        void Awake() { Current = this; }
        void Start() { ambush.EncounterCompleted += AmbushFinished; }
        void Update()
        {
            if (quests == null || !quests.Ready) return;
            if (!initialized)
            {
                initialized = true;
                quests.CompleteExternal("meet");
                if (quests.ExternalDone("introduction"))
                {
                    guide.StopGuide(); guide.enabled = false; YellowCircleTrigger.Mission1Complete = true;
                    PlaceGuide(classroomReturn.position + Vector3.left * 1.5f);
                }
                if (quests.ExternalDone("canteen"))
                {
                    guide.enabled = false;
                    BimaConversationDriver.ConversationDone = true; BimaConversationDriver.CanteenWalkDone = true;
                    conversation.enabled = false; PlaceGuide(canteenReturn.position);
                }
                if (!string.IsNullOrEmpty(quests.Progress.pendingCutscene))
                {
                    quests.Progress.pendingCutscene = ""; quests.Save();
                }
            }
            if (quests.StepIndex != previousStep)
            {
                previousStep = quests.StepIndex;
                if (previousStep == 6 && !quests.ExternalDone("canteen"))
                {
                    if (BimaConversationDriver.ConversationDone) conversation.StartCanteenWalk();
                    else conversation.BeginExistingConversation();
                }
                if (previousStep >= 10 && !movingToBench) { movingToBench = true; PlaceGuide(gardenBench.position); }
            }
            foreach (var point in bimaPoints) if (point != null && point.Id != "Bench") point.transform.position = guide.transform.position;
            if (legacyObjective != null) legacyObjective.enabled = false;
            if (quests.StepIndex == 5 || quests.StepIndex >= 7 || quests.IsModal)
            {
                MissionBanner.SetObjective(quests.Objective);
                var target = quests.Target;
                if (!quests.IsModal && target != null) { GroundMarker.SetTarget(target.transform.position); Minimap.SetMissionTarget(target.transform.position); }
                else { GroundMarker.ClearTarget(); Minimap.ClearMissionTarget(); }
            }
            else
            {
                MissionBanner.SetObjective(quests.StepIndex == 6 ? "Ikuti Bima ke zona kantin" : "Ikuti Bima ke kelas");
                var target = quests.StepIndex == 6 ? canteenReturn : classroomReturn;
                GroundMarker.SetTarget(target.position); Minimap.SetMissionTarget(target.position);
            }
        }
        void AmbushFinished()
        {
            quests.CompleteExternal("corridor"); guide.PauseGuide();
            quests.PlayExternalWordCard("check_bima", guide.ResumeGuide);
        }
        public bool TryClassroomZone()
        {
            if (!initialized || quests.IsModal || !quests.ExternalDone("check_bima") || quests.ExternalDone("introduction")) return false;
            return StartScene(classroomScene, "classroom", classroomReturn.position);
        }
        public bool TryCanteenZone(string scene)
        {
            if (!initialized || quests.IsModal || quests.StepIndex != 6 || !BimaConversationDriver.ConversationDone || quests.ExternalDone("canteen")) return false;
            return StartScene(scene, "canteen", canteenReturn.position);
        }
        bool StartScene(string scene, string id, Vector3 returnPosition)
        {
            if (!string.IsNullOrEmpty(quests.Progress.pendingCutscene) || !Application.CanStreamedLevelBeLoaded(scene)) return false;
            quests.Progress.pendingCutscene = id; quests.Save();
            quests.Progress.position = returnPosition; quests.Progress.hasPosition = true;
            if (!LevelOneCheckpoint.Write(quests.Progress)) { quests.Progress.pendingCutscene = ""; return false; }
            GameStateManager.Instance.SetState(GameState.Cinematic); guide.PauseGuide();
            LoadingScreen.LoadSceneWithLoading(scene); return true;
        }
        void PlaceGuide(Vector3 position)
        {
            var agent = guide.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && NavMesh.SamplePosition(position, out var hit, 3, NavMesh.AllAreas)) agent.Warp(hit.position);
        }
        public static void FinishScene(bool completed)
        {
            var save = LevelOneCheckpoint.Load(13);
            if (string.IsNullOrEmpty(save.pendingCutscene)) return;
            if (completed)
            {
                var ids = save.pendingCutscene == "classroom" ? new[] { "class_front", "introduction" } : new[] { "canteen" };
                save.externalCompleted = (save.externalCompleted ?? new string[0]).Concat(ids).Distinct().ToArray();
            }
            save.pendingCutscene = ""; LevelOneCheckpoint.Write(save);
        }
        void OnDestroy()
        {
            if (ambush != null) ambush.EncounterCompleted -= AmbushFinished;
            if (Current == this) Current = null;
        }
    }
}

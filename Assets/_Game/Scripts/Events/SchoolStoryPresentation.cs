using UnityEngine;
using UnityEngine.AI;
using Unity.Cinemachine;
using BullyingGame.Core;
using BullyingGame.Quest;
using BullyingGame.Dialogue;
using BullyingGame.Save;

namespace BullyingGame.Events
{
    public class SchoolStoryPresentation : MonoBehaviour
    {
        [Header("Scene Actors")]
        [SerializeField] private Transform player, sari, ratih, raka, dion, tio, fajar;
        [SerializeField] private GameObject bag, pencils, privateStage, parents;
        [SerializeField] private CinemachineCamera privateCamera;
        [SerializeField] private QuestData bagMission, supportMission, routeMission;
        [SerializeField] private DialogueData privateSummary;
        private bool summaryShown;
        public bool CanSkipPrivateScene => privateScene && !summaryShown;
        public void SkipPrivateScene()
        {
            if (CanSkipPrivateScene && dialogues.TryShowSummary(dialogues.CurrentDialogue, privateSummary)) summaryShown = true;
        }
        private DialogueManager dialogues;
        private Vector3[] positions;
        private Quaternion[] rotations;
        private Transform[] staged;
        private bool privateScene;
        private Renderer[] playerRenderers;
        private bool[] visibility;
        private void Start(){dialogues=DialogueManager.Instance;if(dialogues!=null){dialogues.OnDialogueStarted+=Stage;dialogues.OnDialogueEnded+=Restore;}privateStage.SetActive(false);}
        private void Update()
        {
            var q=QuestManager.Instance;if(q==null)return;
            if(bag!=null)
            {
                bool retrieved=q.IsObjectiveCompleted(bagMission,"retrieve"),returned=q.IsObjectiveCompleted(bagMission,"return");
                bag.SetActive(q.GetQuestState(bagMission)!=QuestState.Locked||SaveManager.Instance.CurrentData.currentChapter>=4);
                var owner=returned?raka:retrieved?ratih:fajar;
                if(owner!=null)bag.transform.SetPositionAndRotation(owner.position+owner.right*.6f+Vector3.up*.5f,owner.rotation);
            }
            if(pencils!=null)pencils.SetActive(SaveManager.Instance.CurrentData.currentChapter==1&&!q.IsObjectiveCompleted(supportMission,"support_raka"));
            if(parents!=null){var opening=Object.FindAnyObjectByType<BullyingGame.Cinematics.StoryOpeningDirector>();parents.SetActive(SaveManager.Instance.CurrentData.currentChapter==1&&opening!=null&&opening.IsRunning);}
        }
        private void Stage()
        {
            var data=dialogues.CurrentDialogue;if(data==null)return;
            if(data.name=="Dialogue_M13_context")
            {
                privateScene=true;summaryShown=false;privateStage.SetActive(true);privateCamera.Priority=40;
                playerRenderers=player.GetComponentsInChildren<Renderer>(true);visibility=new bool[playerRenderers.Length];
                for(int i=0;i<playerRenderers.Length;i++){visibility[i]=playerRenderers[i].enabled;playerRenderers[i].enabled=false;}return;
            }
            if(data.name.StartsWith("Dialogue_M02_")||data.name=="Dialogue_M06_teacher")
            {
                var anchor=data.name.EndsWith("observe_bima")?GameObject.Find("PRDCast/Bima")?.transform:data.name.EndsWith("observe_laras")?GameObject.Find("PRDCast/Laras")?.transform:data.name.EndsWith("observe_raka")?raka:GameObject.Find("PRDCast/PakBeni")?.transform;
                if(anchor!=null)StageActors(new[]{dion,tio},new[]{anchor.position+new Vector3(2,0,1),anchor.position+new Vector3(3.2f,0,1.5f)});
            }
            else if(data.name=="Dialogue_M11_retrieve"||data.name=="Dialogue_M11_return")
            {
                Transform anchor=data.name.EndsWith("retrieve")?fajar:raka;
                StageActors(new[]{ratih},new[]{anchor.position+new Vector3(1.7f,0,1)});
            }
        }
        private void StageActors(Transform[] actors,Vector3[] destination)
        {
            staged=actors;positions=new Vector3[actors.Length];rotations=new Quaternion[actors.Length];
            for(int i=0;i<actors.Length;i++){positions[i]=actors[i].position;rotations[i]=actors[i].rotation;Place(actors[i],destination[i]);actors[i].rotation=Quaternion.Euler(0,180,0);}
        }
        private static void Place(Transform actor,Vector3 position)
        {
            var agent=actor.GetComponent<NavMeshAgent>();if(agent!=null&&agent.enabled&&agent.isOnNavMesh)agent.Warp(position);else actor.position=position;
        }
        private void Restore()
        {
            if(privateScene){privateScene=false;privateStage.SetActive(false);privateCamera.Priority=0;for(int i=0;i<playerRenderers.Length;i++)if(playerRenderers[i]!=null)playerRenderers[i].enabled=visibility[i];}
            if(staged!=null){for(int i=0;i<staged.Length;i++)if(staged[i]!=null){Place(staged[i],positions[i]);staged[i].rotation=rotations[i];}staged=null;}
        }
        private void OnDestroy(){if(dialogues!=null){dialogues.OnDialogueStarted-=Stage;dialogues.OnDialogueEnded-=Restore;}Restore();}
    }
}

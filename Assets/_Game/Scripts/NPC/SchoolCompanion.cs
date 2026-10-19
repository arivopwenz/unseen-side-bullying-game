using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Core;
using BullyingGame.Save;
using BullyingGame.Quest;

namespace BullyingGame.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class SchoolCompanion : MonoBehaviour
    {
        [Header("Safe orientation and BK waypoints")]
        [SerializeField] private StoryChapterSequence sequence;
        [SerializeField] private Transform player,teacher;
        [SerializeField] private QuestData orientation,report;
        private NavMeshAgent agent;
        private float nextUpdate,blockedAt;
        private Vector3 lastPosition,teacherHome;
        private string phase;
        private void Start(){agent=GetComponent<NavMeshAgent>();lastPosition=transform.position;teacherHome=teacher.position;}
        private void Update()
        {
            if(agent==null||!agent.enabled)return;
            if(GameStateManager.Instance==null||GameStateManager.Instance.CurrentState!=GameState.Playing){if(agent.isOnNavMesh)agent.isStopped=true;return;}
            if(Time.time<nextUpdate)return;nextUpdate=Time.time+.4f;
            var quests=QuestManager.Instance;Vector3 destination=transform.position;string next="idle";
            if(quests.GetQuestState(orientation)==QuestState.Active)
            {
                if(quests.IsObjectiveCompleted(orientation,"guidance")){destination=new Vector3(2,.05f,12);next="teacher";}
                else if(quests.IsObjectiveCompleted(orientation,"corridor")){destination=new Vector3(19,.05f,10);next="bk";}
                else if(quests.IsObjectiveCompleted(orientation,"field")){destination=new Vector3(-1,.05f,7);next="corridor";}
                else if(quests.IsObjectiveCompleted(orientation,"classroom")){destination=new Vector3(-7,.05f,-6);next="field";}
            }
            if(quests.GetQuestState(report)==QuestState.Active&&quests.IsObjectiveCompleted(report,"route")&&!quests.IsObjectiveCompleted(report,"report"))
            {
                int route=SaveManager.Instance.GetNarrativeChoice("M12/route");
                destination=route==1&&player.position.z<22?new Vector3(-10,.05f,24):new Vector3(19,.05f,10);next="route"+route;
                if(route==2&&teacher!=null)
                {
                    Vector3 follow=player.position-player.forward*2+Vector3.right;
                    if(NavMesh.SamplePosition(follow,out var teacherHit,2,NavMesh.AllAreas)&&Vector3.Distance(teacher.position,follow)>3)teacher.position=teacherHit.position;
                }
            }
            if(next=="idle"){if(agent.isOnNavMesh)agent.isStopped=true;return;}
            if(!NavMesh.SamplePosition(destination,out var hit,2,NavMesh.AllAreas))return;
            if(phase!=next){phase=next;blockedAt=Time.time;}
            if(!agent.isOnNavMesh){agent.Warp(hit.position);return;}
            agent.isStopped=false;agent.SetDestination(hit.position);
            if(Vector3.Distance(lastPosition,transform.position)>.15f){blockedAt=Time.time;lastPosition=transform.position;}
            if(Time.time-blockedAt>6&&Vector3.Distance(transform.position,hit.position)>2||Vector3.Distance(transform.position,player.position)>30)
            {agent.Warp(hit.position);blockedAt=Time.time;}
        }
    }
}

using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Core;
using BullyingGame.Quest;

namespace BullyingGame.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EscortCompanion : MonoBehaviour
    {
        [Header("Escort Mission")]
        [SerializeField] private QuestData quest;
        [SerializeField] private string startObjective = "agree_escort";
        [SerializeField] private string arriveObjective = "reach_safe_zone";
        [SerializeField] private Transform player;
        [SerializeField] private Transform safeZone;
        [SerializeField, Min(.5f)] private float followDistance = 1.5f;
        private NavMeshAgent agent;
        private NavMeshPath path;
        private float nextPathUpdate;
        public Transform Destination => safeZone;
        public bool IsEscorting => QuestManager.Instance!=null && QuestManager.Instance.GetQuestState(quest)==QuestState.Active &&
            QuestManager.Instance.IsObjectiveCompleted(quest,startObjective) && !QuestManager.Instance.IsObjectiveCompleted(quest,arriveObjective);
        private void Awake() { agent = GetComponent<NavMeshAgent>(); path = new NavMeshPath(); }
        private void Update()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            var quests = QuestManager.Instance;
            bool active = quests != null && quests.GetQuestState(quest) == QuestState.Active &&
                quests.IsObjectiveCompleted(quest, startObjective) && !quests.IsObjectiveCompleted(quest, arriveObjective);
            if (!active || player == null || safeZone == null || GameStateManager.Instance == null ||
                GameStateManager.Instance.CurrentState != GameState.Playing)
            {
                agent.isStopped = true;
                return;
            }
            if (Vector3.Distance(player.position, safeZone.position) < 2.2f &&
                Vector3.Distance(transform.position, safeZone.position) < 2.2f)
            {
                agent.isStopped = true;
                quests.UpdateObjective(quest, arriveObjective);
                return;
            }
            float distance = Vector3.Distance(player.position, transform.position);
            agent.isStopped = distance < followDistance;
            if (!agent.isStopped && Time.time >= nextPathUpdate)
            {
                nextPathUpdate = Time.time + .25f;
                var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
                if (NavMesh.SamplePosition(player.position, out var hit, 1f, filter) &&
                    NavMesh.CalculatePath(transform.position, hit.position, filter, path) && path.status == NavMeshPathStatus.PathComplete)
                    agent.SetPath(path);
            }
        }
        private void OnDisable()
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); }
        }
    }
}

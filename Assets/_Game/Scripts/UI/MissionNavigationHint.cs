using UnityEngine;
using TMPro;
using BullyingGame.Core;
using BullyingGame.NPC;
using BullyingGame.Quest;
using UnityEngine.AI;

namespace BullyingGame.UI
{
    public class MissionNavigationHint : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private StoryChapterSequence sequence;
        [SerializeField] private Transform player;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private RectTransform directionArrow;
        [SerializeField] private MissionDialogueNPC[] characters;
        [SerializeField] private QuestItem notebook;
        [SerializeField] private EscortCompanion companion;
        private float updateAt;
        private NavMeshPath path;
        private void Awake() => path = new NavMeshPath();
        private void Update()
        {
            if(hintText==null) return;
            if(sequence==null || player==null || sequence.ActiveQuest==null || GameStateManager.Instance==null ||
                GameStateManager.Instance.CurrentState!=GameState.Playing) { hintText.text="";SetArrow(false);return; }
            if(Time.unscaledTime<updateAt) return;
            updateAt=Time.unscaledTime+.25f;
            Transform target=null;
            if(companion!=null && companion.IsEscorting) { target=companion.Destination; }
            else if(notebook!=null && notebook.CanInteract()) { target=notebook.transform; }
            else if(characters!=null) foreach(var npc in characters)
                if(npc!=null && npc.OffersObjective(sequence.ActiveQuest)) { target=npc.transform;break; }
            if(target==null) { hintText.text="";SetArrow(false);return; }
            var camera=UnityEngine.Camera.main;
            var forward=camera!=null ? camera.transform.forward : player.forward;
            forward.y=0;
            var direction=target.position-player.position;direction.y=0;
            float distance = direction.magnitude;
            if (path == null) path = new NavMeshPath();
            Vector3 waypoint = NextWaypoint(player.position, target.position, path);
            direction = waypoint - player.position; direction.y = 0;
            float angle=Vector3.SignedAngle(forward,direction,Vector3.up);
            hintText.text = distance < 2f ? "Tujuan di dekatmu" : distance.ToString("0") + " m";
            SetArrow(distance >= 2f);
            if (directionArrow != null) directionArrow.localRotation = Quaternion.Euler(0, 0, -angle);
        }
        public static Vector3 NextWaypoint(Vector3 origin, Vector3 destination, NavMeshPath route)
        {
            if (NavMesh.SamplePosition(origin, out var start, 2f, NavMesh.AllAreas) &&
                NavMesh.SamplePosition(destination, out var end, 3f, NavMesh.AllAreas) &&
                NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, route) &&
                route.status == NavMeshPathStatus.PathComplete)
                foreach (var corner in route.corners)
                    if (Vector3.ProjectOnPlane(corner - origin, Vector3.up).sqrMagnitude > 1.44f)
                        return corner;
            return destination;
        }
        private void SetArrow(bool visible) { if(directionArrow != null) directionArrow.gameObject.SetActive(visible); }
    }
}

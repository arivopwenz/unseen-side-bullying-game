using UnityEngine;
using TMPro;
using BullyingGame.Core;
using BullyingGame.NPC;
using BullyingGame.Quest;

namespace BullyingGame.UI
{
    public class MissionNavigationHint : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private StoryChapterSequence sequence;
        [SerializeField] private Transform player;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private MissionDialogueNPC[] characters;
        [SerializeField] private QuestItem notebook;
        [SerializeField] private EscortCompanion companion;
        private float updateAt;
        private void Update()
        {
            if(hintText==null) return;
            if(sequence==null || player==null || sequence.ActiveQuest==null || GameStateManager.Instance==null ||
                GameStateManager.Instance.CurrentState!=GameState.Playing) { hintText.text="";return; }
            if(Time.unscaledTime<updateAt) return;
            updateAt=Time.unscaledTime+.25f;
            Transform target=null;string label="";
            if(companion!=null && companion.IsEscorting) { target=companion.Destination;label="Area aman — temani Denis"; }
            else if(notebook!=null && notebook.CanInteract()) { target=notebook.transform;label="Buku catatan"; }
            else if(characters!=null) foreach(var npc in characters)
                if(npc!=null && npc.OffersObjective(sequence.ActiveQuest)) { target=npc.transform;label=npc.GetPromptText();break; }
            if(target==null) { hintText.text="Selesaikan refleksi untuk melanjutkan cerita.";return; }
            var camera=UnityEngine.Camera.main;
            var forward=camera!=null ? camera.transform.forward : player.forward;
            forward.y=0;
            var direction=target.position-player.position;direction.y=0;
            float angle=Vector3.SignedAngle(forward,direction,Vector3.up);
            string side=Mathf.Abs(angle)<35 ? "depan" : Mathf.Abs(angle)>145 ? "belakang" : angle>0 ? "kanan" : "kiri";
            hintText.text=label+"  •  "+direction.magnitude.ToString("0")+" m  •  "+side;
        }
    }
}

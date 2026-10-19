using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using BullyingGame.Core;
using BullyingGame.Save;
using BullyingGame.NPC;

namespace BullyingGame.UI
{
    public class SchoolFieldGuide : MonoBehaviour
    {
        [Header("School Map")]
        [SerializeField] private Transform player;
        [SerializeField] private StoryChapterSequence sequence;
        [SerializeField] private MissionDialogueNPC[] targets;
        private InputAction journalAction,mapAction;
        private bool opened;
        private float previousTime;
        private void Awake()
        {
            journalAction=new InputAction("Journal",InputActionType.Button,"<Keyboard>/j");
            mapAction=new InputAction("Map",InputActionType.Button,"<Keyboard>/m");
            journalAction.performed+=_=>OpenJournal("Sekolah");mapAction.performed+=_=>OpenMap();
        }
        private void OnEnable(){journalAction?.Enable();mapAction?.Enable();}
        private bool Hold()
        {
            if(opened)return true;
            if(GameStateManager.Instance==null||GameStateManager.Instance.CurrentState!=GameState.Playing||StoryOverlay.Current!=null)return false;
            opened=true;previousTime=Time.timeScale;Time.timeScale=0;GameStateManager.Instance.SetState(GameState.Paused);return true;
        }
        private void Release()
        {
            if(!opened)return;opened=false;Time.timeScale=previousTime;
            if(GameStateManager.Instance!=null&&GameStateManager.Instance.CurrentState==GameState.Paused)GameStateManager.Instance.SetState(GameState.Playing);
        }
        public void OpenJournal(string category)
        {
            if(!Hold())return;
            var b=new StringBuilder("Catatan Aji membedakan pengamatan, keterangan dan hal yang belum diketahui. Kamu boleh meminta bantuan guru meski jurnal belum lengkap.\n\n");
            var entries=SaveManager.Instance?.CurrentData.journalEntries;
            if(entries!=null)foreach(var e in entries.Where(e=>(string.IsNullOrEmpty(e.caseId)?"Sekolah":e.caseId)==category))b.Append("<b>"+e.status+"</b>\n"+e.text+"\n\n");
            if(entries==null||!entries.Any(e=>(string.IsNullOrEmpty(e.caseId)?"Sekolah":e.caseId)==category))b.Append("Belum ada catatan pada bagian ini.");
            StoryOverlay.Show("Jurnal · "+category,b.ToString(),new[]{"Sekolah","Bima","Laras","Raka","Tutup"},i=>
            {
                StoryOverlay.Current?.Dismiss();if(i<4)OpenJournal(new[]{"Sekolah","Bima","Laras","Raka"}[i]);
            },Release);
        }
        public void OpenMap()
        {
            if(!Hold())return;
            var view=StoryOverlay.Show("Peta sekolah", "",new[]{"Tutup"},_=>StoryOverlay.Current?.Dismiss(),Release);
            view.Body.gameObject.SetActive(false);
            var plot=StoryOverlay.Rect(view.Content,"SchoolPlan",new Vector2(0,-270),new Vector2(1000,480));plot.anchorMin=plot.anchorMax=new Vector2(.5f,1);plot.gameObject.AddComponent<Image>().color=new Color(.12f,.19f,.2f);
            Mark(plot,"KELAS",new Vector3(-20,0,15),new Vector2(200,95),new Color(.20f,.35f,.4f));
            Mark(plot,"RUANG GURU",new Vector3(5,0,16),new Vector2(180,95),new Color(.2f,.35f,.4f));
            Mark(plot,"BK",new Vector3(21,0,16),new Vector2(210,95),new Color(.2f,.35f,.4f));
            Mark(plot,"KORIDOR  →  BK",new Vector3(0,0,7),new Vector2(830,34),new Color(.24f,.28f,.29f));
            Mark(plot,"LAPANGAN",new Vector3(0,0,-8),new Vector2(370,170),new Color(.19f,.32f,.27f));
            Mark(plot,"Jalur tenang →",new Vector3(12,0,24),new Vector2(300,26),new Color(.2f,.3f,.28f));
            if(player!=null)Mark(plot,"● AJI",player.position,new Vector2(115,45),new Color(.1f,.45f,.55f));
            var target=CurrentTarget();if(target!=null)Mark(plot,"TUJUAN",target.position,new Vector2(140,45),new Color(.5f,.31f,.1f));
            string goal=sequence!=null&&sequence.ActiveQuest!=null?sequence.ActiveQuest.questTitle:"Orientasi sekolah";
            StoryOverlay.Text(view.Content,"Legend","● Aji   Tujuan aktif\n"+goal+"\nPeta tidak menampilkan rahasia konseling atau posisi tersembunyi siswa.",new Vector2(0,-550),new Vector2(1020,110),24).rectTransform.anchorMin=new Vector2(.5f,1);
            var legend=view.Content.Find("Legend").GetComponent<RectTransform>();legend.anchorMax=new Vector2(.5f,1);legend.anchoredPosition=new Vector2(0,-550);
        }
        private static void Mark(Transform plot,string text,Vector3 position,Vector2 size,Color color)
        {
            var rect=StoryOverlay.Rect(plot,text,new Vector2(position.x/64*940,position.z/56*440),size);rect.gameObject.AddComponent<Image>().color=color;
            var label=StoryOverlay.Text(rect,"Name",text,Vector2.zero,size,24*PlayerPrefs.GetFloat("Story.TextScale",1));
            label.alignment=size.y>60?TMPro.TextAlignmentOptions.Top:TMPro.TextAlignmentOptions.Center;
            if(size.y>60)label.margin=new Vector4(0,6,0,0);
        }
        public Transform CurrentTarget()
        {
            if(sequence==null||sequence.ActiveQuest==null||targets==null)return null;
            foreach(var npc in targets)if(npc!=null&&npc.OffersObjective(sequence.ActiveQuest))return npc.transform;
            return null;
        }
        private void OnDisable(){journalAction?.Disable();mapAction?.Disable();if(opened){StoryOverlay.Current?.Dismiss();Release();}}
        private void OnDestroy(){journalAction?.Dispose();mapAction?.Dispose();}
    }
}

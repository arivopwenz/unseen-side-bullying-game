using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Cinemachine;

namespace BullyingGame.UI
{
    public class StorySettings : MonoBehaviour
    {
        private static readonly Dictionary<TMP_Text,float> baseFonts = new Dictionary<TMP_Text,float>();
        private static readonly Dictionary<CinemachineInputAxisController,float[]> baseGains = new Dictionary<CinemachineInputAxisController,float[]>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; Apply(); }
        private static void Loaded(Scene scene, LoadSceneMode mode) { baseFonts.Clear(); baseGains.Clear(); Apply(); }
        public static void Apply()
        {
            AudioListener.volume=PlayerPrefs.GetFloat("MasterVolume",.8f);
            float scale=PlayerPrefs.GetFloat("Story.TextScale",1);
            foreach(var t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(t is not TextMeshProUGUI || t.GetComponentInParent<StoryOverlay>()!=null)continue;
                if(!baseFonts.ContainsKey(t))baseFonts[t]=t.fontSize;
                t.fontSize=baseFonts[t]*scale;
            }
            var sequence=UnityEngine.Object.FindAnyObjectByType<BullyingGame.Core.StoryChapterSequence>();
            if(sequence!=null&&sequence.ActiveQuest!=null)UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>()?.RefreshQuest(sequence.ActiveQuest);
            foreach(var axis in UnityEngine.Object.FindObjectsByType<CinemachineInputAxisController>(FindObjectsSortMode.None))
            {
                if(!baseGains.ContainsKey(axis)){var gains=new float[axis.Controllers.Count];for(int i=0;i<gains.Length;i++)gains[i]=axis.Controllers[i].Input.Gain;baseGains[axis]=gains;}
                for(int i=0;i<axis.Controllers.Count&&i<baseGains[axis].Length;i++)
                    axis.Controllers[i].Input.Gain=baseGains[axis][i]*PlayerPrefs.GetFloat("Story.Sensitivity",1)*(i==1&&PlayerPrefs.GetInt("Story.InvertY",0)==1?-1:1);
            }
        }
        public static void Show(Action onClose=null)
        {
            var panel=StoryOverlay.Show("Pengaturan", "",new[]{"Bantuan kontrol","Selesai"},i=>{if(i==0){StoryOverlay.Current?.Dismiss();Help(()=>Show(onClose));}else StoryOverlay.Current?.Dismiss();},onClose);
            panel.Body.gameObject.SetActive(false);
            Action refresh=()=>{PlayerPrefs.Save();Apply();};
            var volume=StoryOverlay.Button(panel.Content,"",new Vector2(0,-60),new Vector2(990,70),null);
            Action volumeLabel=()=>volume.GetComponentInChildren<TMP_Text>().text="Volume: "+Mathf.RoundToInt(PlayerPrefs.GetFloat("MasterVolume",.8f)*100)+"%  (pilih untuk mengubah)";
            volume.onClick.AddListener(()=>{float v=PlayerPrefs.GetFloat("MasterVolume",.8f)+.2f;PlayerPrefs.SetFloat("MasterVolume",v>1.05f?0:v);refresh();volumeLabel();});volumeLabel();
            var text=StoryOverlay.Button(panel.Content,"",new Vector2(0,-150),new Vector2(990,70),null);
            Action textLabel=()=>text.GetComponentInChildren<TMP_Text>().text="Ukuran teks: "+Mathf.RoundToInt(PlayerPrefs.GetFloat("Story.TextScale",1)*100)+"%";
            text.onClick.AddListener(()=>{float s=PlayerPrefs.GetFloat("Story.TextScale",1);PlayerPrefs.SetFloat("Story.TextScale",s<1.05f?1.15f:s<1.2f?1.3f:1);refresh();textLabel();});textLabel();
            var sensitivity=StoryOverlay.Button(panel.Content,"",new Vector2(0,-240),new Vector2(990,70),null);
            Action sensitivityLabel=()=>sensitivity.GetComponentInChildren<TMP_Text>().text="Sensitivitas kamera: "+PlayerPrefs.GetFloat("Story.Sensitivity",1).ToString("0.0")+"x";
            sensitivity.onClick.AddListener(()=>{float s=PlayerPrefs.GetFloat("Story.Sensitivity",1);PlayerPrefs.SetFloat("Story.Sensitivity",s<.75f?1:s<1.25f?1.5f:.5f);refresh();sensitivityLabel();});sensitivityLabel();
            var invert=StoryOverlay.Button(panel.Content,"",new Vector2(0,-330),new Vector2(990,70),null);
            Action invertLabel=()=>invert.GetComponentInChildren<TMP_Text>().text="Balik sumbu vertikal: "+(PlayerPrefs.GetInt("Story.InvertY",0)==1?"Ya":"Tidak");
            invert.onClick.AddListener(()=>{PlayerPrefs.SetInt("Story.InvertY",1-PlayerPrefs.GetInt("Story.InvertY",0));refresh();invertLabel();});invertLabel();
            var instant=StoryOverlay.Button(panel.Content,"",new Vector2(0,-420),new Vector2(990,70),null);
            Action instantLabel=()=>instant.GetComponentInChildren<TMP_Text>().text="Teks langsung lengkap: "+(PlayerPrefs.GetInt("Story.InstantText",0)==1?"Ya":"Tidak");
            instant.onClick.AddListener(()=>{PlayerPrefs.SetInt("Story.InstantText",1-PlayerPrefs.GetInt("Story.InstantText",0));refresh();instantLabel();});instantLabel();
            foreach(var r in panel.Content.GetComponentsInChildren<RectTransform>()) if(r.parent==panel.Content){r.anchorMin=r.anchorMax=new Vector2(.5f,1);}
        }
        public static void Help(Action onClose=null) => StoryOverlay.Show("Bantuan kontrol",
            "WASD — berjalan\nMouse — putar kamera\nShift — lari ringan\nE — bicara atau berinteraksi\nJ — jurnal fakta\nM — peta sekolah\nEsc — jeda / tutup panel\nEnter atau klik Lanjut — baca dialog\nTombol arah dan Enter — pilih respons/menu\n\nTidak ada batas waktu pilihan bantuan. Guru dapat dimintai bantuan walau catatan belum lengkap. Keluar di tengah adegan kembali ke checkpoint terakhir yang stabil. Kamera dan teks dapat disesuaikan di Pengaturan.",new[]{"Kembali"},_=>StoryOverlay.Current?.Dismiss(),onClose);
    }
}

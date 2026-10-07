using UnityEngine;
using TMPro;
using BullyingGame.Core;
using BullyingGame.Quest;

namespace BullyingGame.UI
{
    public class StoryLearningHint : MonoBehaviour
    {
        [Header("Contextual Guidance")]
        [SerializeField] private StoryChapterSequence sequence;
        [SerializeField] private TMP_Text text;
        private void Update()
        {
            if(text == null) return;
            if(sequence == null || sequence.ActiveQuest == null || GameStateManager.Instance == null || GameStateManager.Instance.CurrentState != GameState.Playing) { text.text="";return; }
            string id=sequence.ActiveQuest.questId;
            if(id=="ch01_meet_denis") text.text="Dekati Denis. Gunakan tombol interaksi yang muncul. Pilihan Aji mengubah respons dan langkah berikutnya.";
            else if(id=="quest_level01_lost_notebook") text.text="Ini buku tugas Denis. Hasil QTE mengubah lokasi buku; kembalikan buku agar misi selesai.";
            else if(id=="ch01_escort") text.text="Berjalanlah bersama Denis menuju area hijau. Tunggu sampai kalian berdua tiba.";
            else if(id=="ch02_arithmetic") text.text="Datangi Pak Bambang di ruang kelas. Soal tidak memakai timer dan tidak memberi penalti nilai.";
            else if(id=="ch02_lunch") text.text="Datangi kantin. Adegan ini memperlihatkan ketimpangan kuasa; hasilnya tidak menilai kemampuan Denis.";
            else if(id=="ch02_pressure") text.text="Efek tekanan menggambarkan perasaan Denis. Efek dan bunyinya bisa dimatikan melalui Pengaturan.";
            else text.text="";
        }
    }
}

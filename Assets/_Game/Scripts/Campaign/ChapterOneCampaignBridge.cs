using UnityEngine;
using BullyingGame.Core;
using BullyingGame.LevelOne;

namespace BullyingGame.Campaign
{
    [RequireComponent(typeof(LevelOneDirector), typeof(LevelOneView))]
    public sealed class ChapterOneCampaignBridge : MonoBehaviour
    {
        LevelOneDirector chapter;
        bool shown;
        void Awake() => chapter = GetComponent<LevelOneDirector>();
        void Update()
        {
            if (shown || !chapter.Ready || !chapter.Complete || chapter.IsModal) return;
            shown = true;
            GameStateManager.Instance.SetState(GameState.Result);
            var view = GetComponent<LevelOneView>();
            view.ShowOptions("Chapter 1 selesai", "Membantu teman berarti mendengar, bertindak dengan aman, " +
                "dan mencari bantuan. Bima berhak merasa aman.\n\nTindakan Pertama • Catatan Bima",
                new[] { "Lanjut ke Chapter 2", "Menu utama" }, index =>
                {
                    chapter.Save();
                    if (chapter.SaveFailed || !CampaignCheckpoint.AcceptChapterOne())
                    {
                        view.ShowLines(new[] { "Progres belum tersimpan. Coba lagi setelah memeriksa penyimpanan." },
                            () => { shown = false; view.HideModal(); });
                        return;
                    }
                    ChapterSceneFlow.Load(index == 0 ? "Level02" : "01_MainMenu");
                });
        }
    }
}

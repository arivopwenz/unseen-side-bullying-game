using BullyingGame.Core;
using BullyingGame.LevelOne;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Playables;

namespace BullyingGame.Campaign
{
    [RequireComponent(typeof(PlayableDirector), typeof(LevelOneView))]
    public sealed class ChapterOneClosing : MonoBehaviour
    {
        PlayableDirector timeline;
        LevelOneView view;
        bool finished;

        void Awake()
        {
            timeline = GetComponent<PlayableDirector>();
            view = GetComponent<LevelOneView>();
        }

        void Start()
        {
            GameStateManager.Instance?.SetState(GameState.Cinematic);
            view.SetGameplayVisible(false);
            timeline.stopped += OnStopped;
            if (timeline.state != PlayState.Playing) timeline.Play();
        }

        void OnStopped(PlayableDirector source) => Finish();

        void LateUpdate()
        {
            if (!finished && timeline.duration > 0 && timeline.time >= timeline.duration - .001)
                Finish();
        }

        void Finish()
        {
            if (finished) return;
            finished = true;
            GameStateManager.Instance?.SetState(GameState.Result);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            ShowResult();
        }

        void ShowResult()
        {
            if (!CampaignCheckpoint.AcceptChapterOne())
            {
                view.ShowOptions("Progres belum tersimpan", "Coba simpan lagi untuk membuka Chapter 2.",
                    new[] { "Coba lagi" }, _ => ShowResult());
                return;
            }
            view.ShowOptions("Chapter 1 selesai", "Aji mengembalikan botol dan bekal Bima.",
                new[] { "Lanjut ke Level 02" }, _ => ChapterSceneFlow.Load("Level02"));
        }

        void OnDisable() { if (timeline != null) timeline.stopped -= OnStopped; }
    }
}

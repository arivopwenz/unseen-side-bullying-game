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
            if (ChapterSceneFlow.CanLoad("Level02") && CampaignCheckpoint.AcceptChapterOne())
            {
                ChapterClosingTransition.Play();
                return;
            }

            GameStateManager.Instance?.SetState(GameState.Result);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            view.ShowOptions("Transisi belum berhasil", "Coba lagi untuk menyimpan progres dan membuka Level 02.",
                new[] { "Coba lagi" }, _ => { finished = false; Finish(); });
        }

        void OnDisable() { if (timeline != null) timeline.stopped -= OnStopped; }
    }
}

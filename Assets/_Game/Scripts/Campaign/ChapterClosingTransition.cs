using System.Collections;
using BullyingGame.Core;
using BullyingGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BullyingGame.Campaign
{
    public sealed class ChapterClosingTransition : MonoBehaviour
    {
        const string NextScene = "Level02";
        const float FadeDuration = .65f;
        const float TitleDuration = 1.3f;

        internal static void Play()
        {
            var root = new GameObject("Chapter Transition", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(root);
            root.AddComponent<ChapterClosingTransition>().StartCoroutine(
                root.GetComponent<ChapterClosingTransition>().Run());
        }

        IEnumerator Run()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            var black = StoryOverlay.Rect(transform, "Black", Vector2.zero, Vector2.zero);
            black.anchorMin = Vector2.zero;
            black.anchorMax = Vector2.one;
            var background = black.gameObject.AddComponent<Image>();
            var title = StoryOverlay.Text(transform, "Chapter Title", "", Vector2.zero, new Vector2(1400, 160), 78);
            title.alignment = TextAlignmentOptions.Center;
            title.raycastTarget = false;

            Cursor.visible = false;
            GameStateManager.Instance?.SetState(GameState.Loading);
            yield return Fade(background, 0, 1);
            yield return ShowTitle(title, "Chapter 1 Complete");

            if (ChapterSceneFlow.Load(NextScene))
            {
                while (SceneManager.GetActiveScene().name != NextScene) yield return null;
                float gameplayTimeScale = Time.timeScale;
                Time.timeScale = 0;
                yield return ShowTitle(title, "Chapter 02");
                yield return Fade(background, 1, 0);
                Time.timeScale = gameplayTimeScale;
            }
            else yield return Fade(background, 1, 0);

            Destroy(gameObject);
        }

        static IEnumerator ShowTitle(TMP_Text title, string text)
        {
            title.text = text;
            yield return Fade(title, 0, 1);
            yield return new WaitForSecondsRealtime(TitleDuration);
            yield return Fade(title, 1, 0);
        }

        static IEnumerator Fade(Graphic graphic, float from, float to)
        {
            var color = graphic is Image ? Color.black : Color.white;
            float elapsed = 0;
            while (elapsed < FadeDuration)
            {
                color.a = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / FadeDuration));
                graphic.color = color;
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            color.a = to;
            graphic.color = color;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using BullyingGame.UI;

namespace BullyingGame.Cinematics
{
    /// <summary>
    /// Dipasang di GameObject "Cinemachine" scene "Cutscene perkenalan kelas".
    /// Saat scene cutscene dimuat (dari YellowCircleTrigger di Level01): putar Director
    /// (Timeline MasukKelas) lalu kembali ke Level01 saat selesai (maks 90 detik).
    /// Fail-safe: bila Director tidak ketemu / asset null, kembali ke Level01 setelah 2 detik
    /// supaya game tidak macet di scene cutscene. Revert: hapus komponen ini dari "Cinemachine".
    /// </summary>
    public class CutsceneAutoReturn : MonoBehaviour
    {
        private const string LevelSceneName = "Level01";
        [Header("Existing authored Timeline — empty uses this object's Director")]
        [SerializeField] private PlayableDirector authoredDirector;
        [SerializeField, Min(5)] private float timeoutSeconds = 180;
        private void Start()
        {
            StartCoroutine(PlayThenReturn());
        }
        private IEnumerator PlayThenReturn()
        {
            yield return null;
            var director = authoredDirector != null ? authoredDirector : GetComponent<PlayableDirector>();
            if (director == null) director = FindAnyObjectByType<PlayableDirector>();
            if (director == null || director.playableAsset == null)
            {
                yield return new WaitForSeconds(2f);
                BullyingGame.LevelOne.LevelOneExistingFlow.FinishScene(false);
                BullyingGame.Core.GameStateManager.Instance?.SetState(BullyingGame.Core.GameState.Loading);
                LoadingScreen.LoadSceneWithLoading(LevelSceneName);
                yield break;
            }
            if (director.state != PlayState.Playing) director.Play();
            float t = 0f;
            double furthestTime = 0;
            while (director != null && director.state == PlayState.Playing && t < timeoutSeconds)
            {
                t += Time.deltaTime;
                furthestTime = System.Math.Max(furthestTime, director.time);
                yield return null;
            }
            bool complete = director != null && t < timeoutSeconds && furthestTime >= director.duration - System.Math.Max(.3, Time.deltaTime * 3);
            if (director != null) director.Stop();
            BullyingGame.LevelOne.LevelOneExistingFlow.FinishScene(complete);
            BullyingGame.Core.GameStateManager.Instance?.SetState(BullyingGame.Core.GameState.Loading);
            LoadingScreen.LoadSceneWithLoading(LevelSceneName);
        }
    }
}

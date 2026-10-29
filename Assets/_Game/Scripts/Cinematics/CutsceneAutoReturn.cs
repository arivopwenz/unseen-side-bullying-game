using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

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
        private void Start()
        {
            StartCoroutine(PlayThenReturn());
        }
        private IEnumerator PlayThenReturn()
        {
            yield return null;
            var director = GetComponent<PlayableDirector>();
            if (director == null) director = FindFirstObjectByType<PlayableDirector>();
            if (director == null || director.playableAsset == null)
            {
                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene(LevelSceneName);
                yield break;
            }
            if (director.state != PlayState.Playing) director.Play();
            float t = 0f;
            while (director != null && director.state == PlayState.Playing && t < 90f)
            {
                t += Time.deltaTime;
                yield return null;
            }
            if (director != null) director.Stop();
            SceneManager.LoadScene(LevelSceneName);
        }
    }
}

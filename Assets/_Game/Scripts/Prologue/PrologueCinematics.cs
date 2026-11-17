using System.Collections;
using BullyingGame.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace BullyingGame.Prologue
{
    public sealed class PrologueCinematics : MonoBehaviour
    {
        public enum Sequence { None, MasukKelas, TanyaSikka }
        public static Sequence Pending { get; private set; }
        public const string InProgressFlag = "prologue.in-progress";

        [SerializeField] private PlayableDirector masukKelas;
        [SerializeField] private PlayableDirector tanyaSikka;
        [SerializeField] private PlayableDirector istirahat;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSequence() => Pending = Sequence.None;

        public static bool Load(Sequence sequence)
        {
            if (!ChapterSceneFlow.CanLoad("Cutscene perkenalan kelas")) return false;
            Pending = sequence;
            return ChapterSceneFlow.Load("Cutscene perkenalan kelas");
        }

        private IEnumerator Start()
        {
            if (Pending == Sequence.None) yield break;
            GameStateManager.Instance?.SetState(GameState.Cinematic);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Pending == Sequence.MasukKelas)
            {
                yield return Play(masukKelas);
                if (ChapterSceneFlow.Load("Perkenalan")) Pending = Sequence.None;
                yield break;
            }

            if (tanyaSikka != null && tanyaSikka.playableAsset != null)
                yield return Play(tanyaSikka);
            yield return Play(istirahat);
            if (ChapterSceneFlow.Load("Level01"))
            {
                var save = Save.SaveManager.Instance;
                if (save != null)
                {
                    save.SetStoryFlag(InProgressFlag, false);
                    save.SaveGame();
                }
                Pending = Sequence.None;
            }
        }

        private static IEnumerator Play(PlayableDirector director)
        {
            if (director == null || director.playableAsset == null)
            {
                Debug.LogError("Timeline prologue belum terpasang.");
                yield break;
            }
            director.extrapolationMode = DirectorWrapMode.None;
            director.time = 0;
            director.Play();
            yield return null;
            while (director.state == PlayState.Playing) yield return null;
            director.Stop();
        }
    }
}

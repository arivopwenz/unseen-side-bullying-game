using UnityEngine;
using BullyingGame.Core;
using BullyingGame.Quest;
using BullyingGame.QTE;

namespace BullyingGame.Audio
{
    public class PrototypeAudioDirector : MonoBehaviour
    {
        [Header("Original Prototype Audio")]
        [SerializeField] private AudioClip ambience;
        [SerializeField] private AudioClip objectiveComplete;
        [SerializeField] private AudioClip qteSuccess;
        [SerializeField] private AudioClip qteRetry;
        private void Start()
        {
            AudioManager.Instance?.PlayBGM(ambience, .16f);
            if (QuestManager.Instance != null) QuestManager.Instance.OnQuestStateChanged += QuestChanged;
            if (QTEManager.Instance != null) QTEManager.Instance.OnQTEEnded += QTEEnded;
            if (GameStateManager.Instance != null) { GameStateManager.Instance.OnStateChanged += StateChanged; StateChanged(GameState.Boot, GameStateManager.Instance.CurrentState); }
        }
        private void QuestChanged(QuestData data, QuestState state)
        {
            if (state == QuestState.Completed) AudioManager.Instance?.PlaySFX(objectiveComplete, .18f);
        }
        private void QTEEnded(QTEResult result)
        {
            var activity=UnityEngine.Object.FindAnyObjectByType<BullyingGame.Events.NarrativeActivityDirector>();
            if(activity != null && activity.IsPressureActive) return;
            if (QTEManager.Instance != null && !QTEManager.Instance.LastQTECancelled)
                AudioManager.Instance?.PlaySFX(result == QTEResult.Success ? qteSuccess : qteRetry, .15f);
        }
        private void StateChanged(GameState previous, GameState state)
        {
            AudioManager.Instance?.SetBGMGain(state == GameState.Playing ? .16f : state == GameState.Paused ? .04f : .08f);
        }
        private void OnDestroy()
        {
            if (QuestManager.Instance != null) QuestManager.Instance.OnQuestStateChanged -= QuestChanged;
            if (QTEManager.Instance != null) QTEManager.Instance.OnQTEEnded -= QTEEnded;
            if (GameStateManager.Instance != null) GameStateManager.Instance.OnStateChanged -= StateChanged;
        }
    }
}

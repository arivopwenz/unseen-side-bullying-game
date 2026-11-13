using UnityEngine;
using UnityEngine.Playables;
using BullyingGame.UI;

namespace BullyingGame.Events
{
    [RequireComponent(typeof(Collider))]
    public class BekalCutsceneTrigger : MonoBehaviour
    {
        [Header("Cutscene kantin yang sudah ada")]
        public PlayableDirector director;
        public string cutsceneSceneName = "BekalCutscene";
        public bool triggerOnce = true;
        private bool triggered;
        private void OnTriggerEnter(Collider other) => TryActivate(other);
        private void OnTriggerStay(Collider other) => TryActivate(other);
        private void TryActivate(Collider other)
        {
            if (triggerOnce && triggered || !other.CompareTag("Player")) return;
            var flow = BullyingGame.LevelOne.LevelOneExistingFlow.Current;
            if (flow != null)
            {
                if (flow.TryCanteenZone(cutsceneSceneName)) triggered = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(cutsceneSceneName) || !Application.CanStreamedLevelBeLoaded(cutsceneSceneName)) return;
            triggered = true;
            LoadingScreen.LoadSceneWithLoading(cutsceneSceneName);
        }
    }
}

using UnityEngine;

namespace BullyingGame.LevelOne
{
    public sealed class BekalCutsceneProps : MonoBehaviour
    {
        [SerializeField] private Renderer bimaLunchBox, bimaBottle, memetLunchBox, rakaBottle;
        [SerializeField] private double transferTime = 28.05;

        public Renderer BimaLunchBox => bimaLunchBox;
        public Renderer BimaBottle => bimaBottle;
        public Renderer MemetLunchBox => memetLunchBox;
        public Renderer RakaBottle => rakaBottle;
        public bool IsConfigured => bimaLunchBox != null && bimaBottle != null && memetLunchBox != null && rakaBottle != null;

        public void Evaluate(double time)
        {
            if (!IsConfigured) return;
            bool transferred = time >= transferTime;
            bimaLunchBox.enabled = bimaBottle.enabled = !transferred;
            memetLunchBox.enabled = rakaBottle.enabled = transferred;
        }
    }
}

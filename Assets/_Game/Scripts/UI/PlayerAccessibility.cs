using UnityEngine;
using UnityEngine.UI;

namespace BullyingGame.UI
{
    public class PlayerAccessibility : MonoBehaviour
    {
        [Header("Preferences")]
        [SerializeField] private Toggle instantText;
        [SerializeField] private Toggle reducedPressure;
        [SerializeField] private Toggle assistedQTE;
        public static bool InstantText => PlayerPrefs.GetInt("Story.InstantText", 0) == 1;
        public static bool ReducedPressure => PlayerPrefs.GetInt("Story.ReducedPressure", 0) == 1;
        public static bool AssistedQTE => PlayerPrefs.GetInt("Story.AssistedQTE", 0) == 1;
        private void Start()
        {
            Bind(instantText, "Story.InstantText");
            Bind(reducedPressure, "Story.ReducedPressure");
            Bind(assistedQTE, "Story.AssistedQTE");
        }
        private static void Bind(Toggle toggle, string key)
        {
            if(toggle == null) return;
            toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(key, 0) == 1);
            toggle.onValueChanged.AddListener(value => { PlayerPrefs.SetInt(key, value ? 1 : 0); PlayerPrefs.Save(); });
        }
    }
}

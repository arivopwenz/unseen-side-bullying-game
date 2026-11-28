using UnityEngine;

namespace BullyingGame.UI
{
    /// <summary>
    /// Lightweight ScriptableObject stored in Resources/LoadingScreenConfig
    /// that references the single canonical Assets/_Game/Prefabs/UI/LoadingScreen.prefab.
    /// </summary>
    [CreateAssetMenu(menuName = "Unseen Side/UI/Loading Screen Config", fileName = "LoadingScreenConfig")]
    public class LoadingScreenConfig : ScriptableObject
    {
        public GameObject loadingScreenPrefab;
    }
}

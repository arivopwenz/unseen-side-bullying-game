using UnityEngine;
using BullyingGame.Core;

namespace BullyingGame.UI
{
    public class WorldCharacterLabel : MonoBehaviour
    {
        private UnityEngine.Camera view;
        private Renderer labelRenderer;
        private void Awake() => labelRenderer = GetComponent<Renderer>();
        private void LateUpdate()
        {
            if (labelRenderer != null)
                labelRenderer.enabled = GameStateManager.Instance == null ||
                    GameStateManager.Instance.CurrentState == GameState.Playing;
            if (view == null) view = UnityEngine.Camera.main;
            if (view != null) transform.rotation = view.transform.rotation;
        }
    }
}

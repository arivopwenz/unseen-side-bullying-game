using UnityEngine;

namespace BullyingGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        private Rect lastArea;
        private Vector2Int lastSize;
        private void LateUpdate()
        {
            var area = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (size == lastSize && area == lastArea) return;
            lastSize = size; lastArea = area;
            if (size.x <= 0 || size.y <= 0) return;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
            rect.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}

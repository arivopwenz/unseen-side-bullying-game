using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BullyingGame.LevelOne
{
    public sealed class WordTileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<Vector2> Dropped;
        private Vector3 origin, offset;

        public void OnBeginDrag(PointerEventData e)
        {
            origin = transform.position;
            offset = origin - (Vector3)e.position;
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData e) => transform.position = (Vector3)e.position + offset;

        public void OnEndDrag(PointerEventData e)
        {
            transform.position = origin;
            Dropped?.Invoke(e.position);
        }
    }
}

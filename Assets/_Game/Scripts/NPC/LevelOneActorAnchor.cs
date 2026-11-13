using UnityEngine;
namespace BullyingGame.LevelOne
{
    public class LevelOneActorAnchor : MonoBehaviour
    {
        [Header("Actor follows this movable marker")]
        [SerializeField] private Transform point;
        [SerializeField] private bool onlyAtStart;
        void Start() { Align(); }
        void LateUpdate() { if (!onlyAtStart) Align(); }
        void Align() { if (point != null) transform.SetPositionAndRotation(point.position, point.rotation); }
    }
}

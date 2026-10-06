using UnityEngine;

namespace BullyingGame.QTE
{
    [CreateAssetMenu(fileName = "NewQTE", menuName = "Game/QTE Data")]
    public class QTEData : ScriptableObject
    {
        public string qteId;
        public QTEType qteType;
        public float timeLimit = 3f;
        public int requiredPressCount = 10;
        [SerializeField, Min(0.1f)] private float holdDuration = 1f;
        public float HoldDuration => holdDuration;
        [HideInInspector]
        public KeyCode[] sequenceKeys;
    }
}

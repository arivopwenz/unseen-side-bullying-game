namespace BullyingGame.Quest
{
    [System.Serializable]
    public class QuestItemCheckpoint
    {
        public string itemId;
        public QuestItemState state;
        public UnityEngine.Vector3 position;
    }

    public enum QuestItemState
    {
        Hidden,
        Available,
        Collected,
        Stolen,
        Rehidden,
        Secured
    }
}

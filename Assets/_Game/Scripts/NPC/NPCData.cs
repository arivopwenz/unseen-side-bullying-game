using UnityEngine;

namespace BullyingGame.NPC
{
    [CreateAssetMenu(fileName = "NewNPCData", menuName = "Game/NPC Data")]
    public class NPCData : ScriptableObject
    {
        public string npcName;
        public string description;
        public string interactionPrompt = "Bicara";
    }
}

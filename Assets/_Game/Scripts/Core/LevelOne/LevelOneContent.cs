using System;
using UnityEngine;
using BullyingGame.Quest;

namespace BullyingGame.LevelOne
{
    public enum LevelOneAction { Observe, Ask, Inspect, Take, Return, Record }
    [Serializable]
    public class LevelOneStep
    {
        public string id, nodeId, objective;
        public int questIndex;
        public LevelOneAction action;
        [TextArea] public string[] lines;
        public WordCardData puzzle;
        [TextArea] public string journal;
        public bool search;
    }
    [CreateAssetMenu(menuName = "Game/Level One/Content")]
    public class LevelOneContent : ScriptableObject
    {
        [Header("PRD Level 1 — Langkah Pertama")]
        [SerializeField] private QuestData[] quests;
        [SerializeField] private LevelOneStep[] steps;
        [SerializeField, TextArea] private string reflection;
        public QuestData[] Quests => quests;
        public LevelOneStep[] Steps => steps;
        public string Reflection => reflection;
    }
}

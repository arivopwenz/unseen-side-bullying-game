using System;
using UnityEngine;
using BullyingGame.LevelOne;
using BullyingGame.Quest;

namespace BullyingGame.Campaign
{
    public enum CampaignTaskKind { Talk, Order, Classify, Choice, Evidence, Evade, Pickup, Return, Cinematic, Station, DialogueEncounter, LibraryEscort }

    [Serializable]
    public class CampaignQuestion
    {
        public string id, text, feedback;
        public string[] options;
        public int answer;
    }

    [Serializable]
    public class CampaignStep
    {
        public string id, title, objective, actor, pointId, insight, scenePath;
        public string[] lines;
        public string[] afterLines;
        public CampaignTaskKind kind;
        public WordCardData words;
        public CampaignQuestion[] questions;
        public string[] order;
        public Vector3 position;
        public QuestData quest;
        public string choiceKey;
        public bool actionCard;
    }

    [CreateAssetMenu(menuName = "Game/Campaign/Chapter")]
    public sealed class CampaignChapterData : ScriptableObject
    {
        public int chapter;
        public string title, reflection, nextScene;
        public Vector3 spawn;
        public CampaignStep[] steps;
    }
}

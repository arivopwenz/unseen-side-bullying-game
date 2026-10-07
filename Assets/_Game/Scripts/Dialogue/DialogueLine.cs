using System;
using UnityEngine;

namespace BullyingGame.Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)] public string text;
        public AudioClip voiceClip;
    }
}

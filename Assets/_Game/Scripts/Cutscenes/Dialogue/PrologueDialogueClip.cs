using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace UnseenSide.Cutscenes
{
    [System.Serializable]
    public sealed class PrologueDialogueBehaviour : PlayableBehaviour
    {
        public string speaker;
        public string line;
        public string sequence;
    }
    public sealed class PrologueDialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public string speaker = "AYAH";
        [TextArea(2,5)] public string dialogue;
        public string sequence = "01 / 10";
        public ClipCaps clipCaps { get { return ClipCaps.None; } }
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var p = ScriptPlayable<PrologueDialogueBehaviour>.Create(graph);
            var b = p.GetBehaviour();
            b.speaker = speaker; b.line = dialogue; b.sequence = sequence;
            return p;
        }
    }
}
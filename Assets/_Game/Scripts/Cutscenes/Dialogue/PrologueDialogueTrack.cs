using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
namespace UnseenSide.Cutscenes
{
    [TrackColor(0.96f,0.72f,0.30f)]
    [TrackClipType(typeof(PrologueDialogueClip))]
    [TrackBindingType(typeof(PrologueDialoguePanel))]
    public sealed class PrologueDialogueTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            var panel = director ? director.GetGenericBinding(this) as PrologueDialoguePanel : null;
            if (panel) panel.Bind(director);
            return ScriptPlayable<PrologueDialogueMixer>.Create(graph, inputCount);
        }
    }
    public sealed class PrologueDialogueMixer : PlayableBehaviour
    {
        private PrologueDialoguePanel panel;
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            panel = playerData as PrologueDialoguePanel;
            if (!panel) return;
            int selected = -1;
            float best = 0;
            for (int i=0;i<playable.GetInputCount();i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight > best) { best = weight; selected = i; }
            }
            if (selected < 0) { panel.Hide(); return; }
            var input = (ScriptPlayable<PrologueDialogueBehaviour>)playable.GetInput(selected);
            var b = input.GetBehaviour();
            double time = input.GetTime();
            double remaining = input.GetDuration() - time;
            float fade = Mathf.Clamp01((float)(time / 0.12)) * Mathf.Clamp01((float)(remaining / 0.18));
            panel.Show(b.speaker,b.line,b.sequence,fade * best);
        }
        public override void OnGraphStop(Playable playable) { if (panel) panel.Hide(); }
        public override void OnPlayableDestroy(Playable playable) { if (panel) panel.Hide(); }
    }
}
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BullyingGame.LevelOne
{
    [TrackClipType(typeof(BekalPropsClip))]
    [TrackBindingType(typeof(BekalCutsceneProps))]
    public sealed class BekalPropsTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject owner, int inputCount) =>
            ScriptPlayable<Mixer>.Create(graph, inputCount);

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            base.GatherProperties(director, driver);
            if (!(director.GetGenericBinding(this) is BekalCutsceneProps props) || !props.IsConfigured) return;
            driver.AddFromName(props.BimaLunchBox, "m_Enabled");
            driver.AddFromName(props.BimaBottle, "m_Enabled");
            driver.AddFromName(props.MemetLunchBox, "m_Enabled");
            driver.AddFromName(props.RakaBottle, "m_Enabled");
        }

        private sealed class Mixer : PlayableBehaviour
        {
            public override void ProcessFrame(Playable playable, FrameData info, object playerData)
            {
                if (playerData is BekalCutsceneProps props) props.Evaluate(playable.GetTime());
            }
        }
    }
}

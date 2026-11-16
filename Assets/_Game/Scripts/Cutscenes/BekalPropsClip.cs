using UnityEngine;
using UnityEngine.Playables;

namespace BullyingGame.LevelOne
{
    public sealed class BekalPropsClip : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) => Playable.Create(graph);
    }
}

using UnityEngine;
namespace BullyingGame.LevelOne
{
    public class LevelOneItemVisuals : MonoBehaviour
    {
        [Header("Item state drives visibility")]
        [SerializeField] private LevelOneDirector director;
        [SerializeField] private GameObject bottle, lunch;
        void Update()
        {
            if (director == null || !director.Ready) return;
            if (bottle != null)
            {
                bool visible = director.IsDayTwo
                    ? director.Searching || director.Step?.id == "greet" || director.Step?.id == "incident" ||
                        director.Step?.id == "clue" || director.Progress.bottleTaken
                    : director.Searching && director.Progress.bottleFound && !director.Progress.bottleTaken;
                if (bottle.activeSelf != visible) bottle.SetActive(visible);
            }
            if (lunch != null)
                lunch.SetActive(!director.IsDayTwo && director.Searching && director.Progress.lunchFound && !director.Progress.lunchTaken);
        }
    }
}

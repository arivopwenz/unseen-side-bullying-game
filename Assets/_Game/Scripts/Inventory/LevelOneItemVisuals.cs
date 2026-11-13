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
            bottle.SetActive(director.Searching && director.Progress.bottleFound && !director.Progress.bottleTaken);
            lunch.SetActive(director.Searching && director.Progress.lunchFound && !director.Progress.lunchTaken);
        }
    }
}

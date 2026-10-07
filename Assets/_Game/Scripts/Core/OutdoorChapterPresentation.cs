using UnityEngine;

namespace BullyingGame.Core
{
    public class OutdoorChapterPresentation : MonoBehaviour
    {
        [Header("Five Outdoor Chapters")]
        [SerializeField] private GameObject[] outdoorSets;
        [SerializeField] private Renderer ground;
        [SerializeField] private Material[] groundMaterials;
        [SerializeField] private GameObject schoolBuilding;
        [SerializeField] private Light sunlight;
        public void Apply(int chapterIndex)
        {
            if (outdoorSets != null)
                for (int i=0; i<outdoorSets.Length; i++) if(outdoorSets[i]!=null) outdoorSets[i].SetActive(i==chapterIndex);
            if(ground!=null && groundMaterials!=null && chapterIndex>=0 && chapterIndex<groundMaterials.Length && groundMaterials[chapterIndex]!=null)
                ground.sharedMaterial=groundMaterials[chapterIndex];
            if(schoolBuilding!=null) schoolBuilding.SetActive(chapterIndex!=3);
            if(sunlight!=null)
            {
                sunlight.color=chapterIndex==4 ? new Color(1,.86f,.69f) : new Color(1,.96f,.88f);
                sunlight.intensity=chapterIndex==4 ? 1.05f : 1.25f;
            }
        }
    }
}

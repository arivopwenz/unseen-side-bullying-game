using System;
using System.Linq;
using UnityEngine;

namespace BullyingGame.Campaign
{
    [Serializable]
    public class CampaignWorldProp
    {
        public string id;
        public GameObject visual;
        public string revealAfter, carryAfter, returnAfter;
        public Transform carryGrip, returnGrip;
        [NonSerialized] public Transform home;
        [NonSerialized] public Vector3 position, scale;
        [NonSerialized] public Quaternion rotation;
    }

    public sealed class CampaignWorldState : MonoBehaviour
    {
        [SerializeField] private CampaignWorldProp[] props = Array.Empty<CampaignWorldProp>();
        public void Initialize()
        {
            foreach (var prop in props)
            {
                var t = prop.visual.transform;
                prop.home = t.parent;
                prop.position = t.localPosition;
                prop.rotation = t.localRotation;
                prop.scale = t.localScale;
            }
        }

        public void RevealGlasses(Vector3 position)
        {
            var glasses = Array.Find(props, prop => prop.id == "glasses");
            if (glasses?.visual == null) return;
            glasses.visual.transform.position = position;
            glasses.position = glasses.home.InverseTransformPoint(position);
            glasses.visual.SetActive(true);
        }

        public void Apply(CampaignProgress data)
        {
            foreach (var prop in props)
            {
                bool Has(string id) => !string.IsNullOrEmpty(id) && data.completedStepIds.Contains(id);
                prop.visual.SetActive(string.IsNullOrEmpty(prop.revealAfter) || Has(prop.revealAfter));
                if (prop.id == "sikka-art") prop.visual.SetActive(Has(prop.revealAfter) && data.exhibitChoice == "SHOW");
                var t = prop.visual.transform;
                Transform parent = Has(prop.returnAfter) ? prop.returnGrip : Has(prop.carryAfter) ? prop.carryGrip : prop.home;
                if (parent == null) parent = prop.home;
                t.SetParent(parent, false);
                t.localPosition = parent == prop.home ? prop.position : Vector3.zero;
                t.localRotation = prop.rotation;
                t.localScale = prop.scale;
            }
        }
    }
}

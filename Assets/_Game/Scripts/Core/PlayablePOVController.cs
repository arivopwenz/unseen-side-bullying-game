using System;
using UnityEngine;

namespace BullyingGame.Core
{
    [Serializable]
    public struct PlayablePerspective
    {
        [SerializeField] private POVType perspective;
        [SerializeField] private Transform spawn;
        [SerializeField] private GameObject npcProxy;
        [SerializeField] private Material uniformMaterial;
        public POVType Perspective => perspective;
        public Transform Spawn => spawn;
        public GameObject Proxy => npcProxy;
        public Material Uniform => uniformMaterial;
    }
    public class PlayablePOVController : MonoBehaviour
    {
        [Header("One Player Controller, Three Character Roles")]
        [SerializeField] private GameObject player;
        [SerializeField] private Renderer[] uniformRenderers;
        [SerializeField] private PlayablePerspective[] perspectives;
        [SerializeField] private TMPro.TMP_Text characterLabel;
        public void SetDisplayName(string name) { if (characterLabel != null) characterLabel.text = name; }
        public void Apply(POVType perspective, bool moveToSpawn = true)
        {
            if (player == null || perspectives == null) return;
            foreach (var slot in perspectives)
            {
                if (slot.Proxy != null) slot.Proxy.SetActive(slot.Perspective != perspective);
                if (slot.Perspective != perspective) continue;
                if (moveToSpawn && slot.Spawn != null) Teleport(slot.Spawn.position, slot.Spawn.rotation);
                if (slot.Uniform != null && uniformRenderers != null)
                    foreach (var renderer in uniformRenderers) if (renderer != null) renderer.sharedMaterial = slot.Uniform;
            }
            POVManager.Instance?.SetPOV(perspective);
        }
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (player == null) return;
            var controller = player.GetComponent<CharacterController>();
            bool enabled = controller != null && controller.enabled;
            if (enabled) controller.enabled = false;
            player.transform.SetPositionAndRotation(position, rotation);
            if (enabled) controller.enabled = true;
            Physics.SyncTransforms();
        }
    }
}

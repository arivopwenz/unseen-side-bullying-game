using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using BullyingGame.Core;
using BullyingGame.LevelOne;

namespace BullyingGame.Campaign
{
    public sealed class CampaignEvade : MonoBehaviour
    {
        [SerializeField] private Transform player, memet, safeZone, startPoint;
        readonly InputAction dodge = new InputAction("Dodge", InputActionType.Button);
        Action<bool> completed;
        bool active, telegraphing, easy;
        float elapsed, nextThreat, dodgeUntil, invulnerableUntil;
        int hits, threats;
        Vector3 runOrigin;


        public InputAction Dodge => dodge;
        public bool Running => active;
        public int Hits => hits;
        void Awake()
        {
            dodge.AddBinding("<Keyboard>/leftCtrl");
            dodge.AddBinding("<Gamepad>/buttonEast");
        }
        public void Begin(bool assisted, Action<bool> callback)
        {
            easy = assisted;
            completed = callback;
            hits = threats = 0;
            active = true;
            telegraphing = false;
            elapsed = dodgeUntil = 0f;
            nextThreat = 1.8f;
            invulnerableUntil = nextThreat;
            runOrigin = startPoint.position;
            Teleport(runOrigin);
            memet.gameObject.SetActive(true);
            memet.position = runOrigin + player.forward * 2.2f;
            dodge.Enable();
            GameStateManager.Instance.SetState(GameState.Playing);
        }
        void Update()
        {
            if (!active || GameStateManager.Instance.CurrentState != GameState.Playing) return;
            elapsed += Time.deltaTime;
            float now = elapsed;
            if (dodge.WasPressedThisFrame() && now >= dodgeUntil)
            {
                dodgeUntil = now + (easy ? 1.5f : .9f);
                var controller = player.GetComponent<CharacterController>();
                controller.Move(player.right * .9f);
            }
            float distance = Vector3.Distance(player.position, safeZone.position);
            if (distance < 2.3f) { Complete(true); return; }
            if (Vector3.Distance(player.position, runOrigin) > 42f) { Complete(false); return; }
            if (now < nextThreat || threats >= 3) return;
            if (!telegraphing)
            {
                telegraphing = true;
                nextThreat = now + (easy ? 1.8f : .7f);
                Vector3 offset = Vector3.ProjectOnPlane(safeZone.position - player.position, Vector3.up).normalized * 1.3f;
                if (NavMesh.SamplePosition(player.position + offset, out var hit, 2, NavMesh.AllAreas))
                    memet.position = hit.position;
                else memet.position = player.position + offset;
                memet.LookAt(new Vector3(player.position.x, memet.position.y, player.position.z));
            }
            else
            {
                threats++;
                if (now > invulnerableUntil && now > dodgeUntil && Vector3.Distance(player.position, memet.position) < 1.9f)
                {
                    hits++;
                    invulnerableUntil = now + 1.5f;
                }
                telegraphing = false;
                nextThreat = now + 2f;
                if (hits >= 2) Complete(false);
            }
        }
        public string Instruction(string binding) => "Menuju Bu Lilis • " +
            (telegraphing ? "Memet bersiap mendekat. [" + binding + "] Menghindar" : "Jaga jarak dan menuju penanda aman") +
            " • Peringatan " + hits + "/2";
        void Complete(bool success)
        {
            if (!active) return;
            active = false;
            dodge.Disable();
            memet.gameObject.SetActive(false);
            var callback = completed;
            completed = null;
            callback?.Invoke(success);
        }
        void Teleport(Vector3 position)
        {
            var controller = player.GetComponent<CharacterController>();
            bool enabled = controller.enabled;
            controller.enabled = false;
            player.position = position;
            controller.enabled = enabled;
        }
        void OnDisable() { active = false; dodge.Disable(); }
        void OnDestroy() => dodge.Dispose();
    }
}

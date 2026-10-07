using System;
using UnityEngine;
using UnityEngine.AI;
using BullyingGame.Core;

namespace BullyingGame.NPC
{
    public enum BullyGroupState { Idle, Approaching, Confronting, Returning }

    public class BullyGroupController : MonoBehaviour
    {
        [Serializable]
        private struct Member
        {
            [SerializeField] private BullyNPC npc;
            [SerializeField] private Transform confrontationPoint;
            public BullyNPC NPC => npc;
            public Transform Point => confrontationPoint;
        }

        [Header("Group and Formation")]
        [SerializeField] private Member[] members = new Member[0];
        [SerializeField] private Transform lookTarget;
        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float approachTimeout = 12f;
        [SerializeField, Min(0.1f)] private float returnTimeout = 12f;

        public event Action<bool> OnApproachCompleted;
        public BullyGroupState CurrentState { get; private set; }
        public string LastFailure { get; private set; }
        private float elapsed;

        public bool TryStartApproach()
        {
            if (!isActiveAndEnabled || CurrentState != BullyGroupState.Idle) return false;
            LastFailure = "";
            if (lookTarget == null || members == null || members.Length == 0)
                return Reject("Isi Look Target dan Members pada Bully Group Controller.");

            // Validate every member before moving anyone, so partial wiring cannot start half a group.
            var paths = new NavMeshPath[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                if (member.NPC == null || member.Point == null)
                    return Reject($"Members Element {i}: NPC dan Confrontation Point wajib diisi.");
                Vector3 delta = member.Point.position - lookTarget.position;
                delta.y = 0;
                if (delta.magnitude < member.NPC.Radius + 0.6f)
                    return Reject($"{member.Point.name}: titik terlalu dekat dengan pemain; beri jarak minimal 1.5 meter.");
                for (int j = 0; j < i; j++)
                {
                    if (member.NPC == members[j].NPC)
                        return Reject($"Members Element {i}: NPC yang sama terpasang dua kali.");
                    delta = member.Point.position - members[j].Point.position;
                    delta.y = 0;
                    if (delta.magnitude < member.NPC.Radius + members[j].NPC.Radius + 0.2f)
                        return Reject("Confrontation Points terlalu berdekatan; pisahkan agar NPC tidak saling menghalangi.");
                }
                if (!member.NPC.TryPrepare(member.Point.position, out paths[i], out var reason))
                    return Reject(reason);
            }

            CurrentState = BullyGroupState.Approaching;
            elapsed = 0;
            for (int i = 0; i < members.Length; i++)
                if (!members[i].NPC.BeginApproach(paths[i], lookTarget))
                {
                    CompleteApproach(false, $"{members[i].NPC.name}: agent gagal memulai perjalanan.");
                    return false;
                }
            return true;
        }

        private bool Reject(string reason)
        {
            LastFailure = reason;
            return false;
        }

        private void Update()
        {
            if (CurrentState != BullyGroupState.Approaching && CurrentState != BullyGroupState.Returning) return;
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Paused) return;
            elapsed += Time.deltaTime;
            bool allArrived = true;
            foreach (var member in members)
            {
                if (member.NPC == null || !member.NPC.isActiveAndEnabled || member.NPC.HasPathFailed)
                {
                    if (CurrentState == BullyGroupState.Approaching)
                        CompleteApproach(false, "NPC/agent kehilangan jalur atau dinonaktifkan saat mendekat.");
                    else StopMembers();
                    return;
                }
                allArrived &= CurrentState == BullyGroupState.Approaching ?
                    member.NPC.HasArrived : !member.NPC.IsMoving;
            }
            if (allArrived)
            {
                if (CurrentState == BullyGroupState.Approaching) CompleteApproach(true, "");
                else StopMembers();
            }
            else if (elapsed >= (CurrentState == BullyGroupState.Approaching ? approachTimeout : returnTimeout))
            {
                if (CurrentState == BullyGroupState.Approaching)
                    CompleteApproach(false, "Waktu mendekat habis. Periksa jarak, NavMesh, dan halangan pada jalur NPC.");
                else StopMembers();
            }
        }

        private void CompleteApproach(bool success, string reason)
        {
            if (CurrentState != BullyGroupState.Approaching) return;
            LastFailure = reason;
            if (success) CurrentState = BullyGroupState.Confronting;
            else EndEncounter();
            OnApproachCompleted?.Invoke(success);
        }

        public void EndEncounter()
        {
            if (CurrentState == BullyGroupState.Idle || CurrentState == BullyGroupState.Returning) return;
            if (!isActiveAndEnabled) { StopMembers(); return; }
            CurrentState = BullyGroupState.Returning;
            elapsed = 0;
            foreach (var member in members)
                if (member.NPC != null) member.NPC.ReturnHome();
        }

        private void StopMembers()
        {
            if (members != null)
                foreach (var member in members)
                    if (member.NPC != null) member.NPC.Release();
            CurrentState = BullyGroupState.Idle;
        }

        private void OnDisable()
        {
            bool wasApproaching = CurrentState == BullyGroupState.Approaching;
            StopMembers();
            if (wasApproaching)
            {
                LastFailure = "Bully Group Controller dinonaktifkan saat mendekat.";
                OnApproachCompleted?.Invoke(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (members == null) return;
            Gizmos.color = new Color(1f, 0.65f, 0.2f);
            foreach (var member in members)
            {
                if (member.Point == null) continue;
                Gizmos.DrawWireSphere(member.Point.position, 0.35f);
                if (member.NPC != null) Gizmos.DrawLine(member.NPC.transform.position, member.Point.position);
                if (lookTarget != null) Gizmos.DrawLine(member.Point.position, lookTarget.position);
            }
        }
    }
}

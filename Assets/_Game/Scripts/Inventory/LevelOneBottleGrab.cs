using System;
using System.Collections;
using UnityEngine;

namespace BullyingGame.LevelOne
{
    public sealed class LevelOneBottleGrab : MonoBehaviour
    {
        [Header("One bottle and two right-hand grips")]
        [SerializeField] private LevelOneDirector director;
        [SerializeField] private Transform bottle, player, bima;
        [SerializeField] private Transform ajiArm, ajiForearm, ajiHand, ajiGrip;
        [SerializeField] private Transform bimaArm, bimaForearm, bimaHand, bimaGrip;

        Quaternion bottleRotation;
        bool busy, received, settlingGrip;
        Vector3 gripOffset;
        float reachWeight;
        Vector3 reach;
        bool handingOver;

        void Awake() => bottleRotation = bottle.rotation;

        public void PickUp(Action finished) => StartCoroutine(Grab(false, finished));
        public void HandOver(Action finished) => StartCoroutine(Grab(true, finished));

        IEnumerator Grab(bool transfer, Action finished)
        {
            busy = true;
            handingOver = transfer;
            reach = transfer ? PrepareHandoff() : PreparePickup();
            yield return BlendReach(0f, 1f, .34f);
            bottle.SetParent(transfer ? bimaGrip : ajiGrip, true);
            gripOffset = bottle.localPosition;
            received = transfer;
            settlingGrip = true;
            yield return BlendReach(1f, 0f, .3f);
            bottle.localPosition = Vector3.zero;
            settlingGrip = false;
            busy = false;
            finished();
        }

        Vector3 PreparePickup()
        {
            var renderer = bottle.GetComponentInChildren<Renderer>();
            Vector3 target = renderer != null ? renderer.bounds.center : bottle.position;
            Approach(target, .5f);
            return target;
        }

        Vector3 PrepareHandoff()
        {
            Approach(bima.position, .82f);
            Face(bima, player.position);
            Vector3 contact = (ajiArm.position + bimaArm.position) * .5f;
            contact.y -= .22f;
            return contact;
        }

        void Approach(Vector3 target, float distance)
        {
            Vector3 delta = Vector3.ProjectOnPlane(target - player.position, Vector3.up);
            var controller = player.GetComponent<CharacterController>();
            if (controller != null && controller.enabled && delta.magnitude > distance)
                controller.Move(delta.normalized * Mathf.Min(1f, delta.magnitude - distance));
            Face(player, target);
        }

        static void Face(Transform actor, Vector3 target)
        {
            Vector3 direction = Vector3.ProjectOnPlane(target - actor.position, Vector3.up);
            if (direction.sqrMagnitude > .001f) actor.rotation = Quaternion.LookRotation(direction);
        }

        IEnumerator BlendReach(float start, float end, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                reachWeight = Mathf.Lerp(start, end, Mathf.SmoothStep(0f, 1f, time / duration));
                yield return null;
                time += Mathf.Min(Time.unscaledDeltaTime, .08f);
            }
            reachWeight = end;
        }

        void LateUpdate()
        {
            if (!director.Ready) return;
            bool hasBottle = director.Progress.bottleTaken;
            if (!busy && hasBottle)
            {
                received |= director.Progress.returned;
                var grip = received ? bimaGrip : ajiGrip;
                if (bottle.parent != grip) Attach(grip);
            }

            bool carry = bottle.parent == ajiGrip;
            if (carry || busy)
            {
                Vector3 hold = ajiArm.position + player.forward * .26f - Vector3.up * .25f;
                PoseArm(ajiArm, ajiForearm, ajiHand, player.right, hold, carry ? .85f : 0f, busy ? reachWeight : 0f);
            }
            if (received || busy && handingOver)
            {
                Vector3 hold = bimaArm.position + bima.forward * .24f - Vector3.up * .22f;
                PoseArm(bimaArm, bimaForearm, bimaHand, bima.right, hold, received ? .85f : 0f, busy ? reachWeight : 0f);
            }
            if (settlingGrip) bottle.localPosition = gripOffset * reachWeight;
            if (carry) bottle.rotation = player.rotation * bottleRotation;
            else if (bottle.parent == bimaGrip) bottle.localRotation = Quaternion.identity;
        }

        void PoseArm(Transform upper, Transform lower, Transform hand, Vector3 right, Vector3 hold, float holdWeight, float interactionWeight)
        {
            ReachArm(upper, lower, hand, right,
                Vector3.Lerp(Vector3.Lerp(hand.position, hold, holdWeight), reach, interactionWeight));
        }

        internal static void ReachArm(Transform upper, Transform lower, Transform hand, Vector3 elbowSide, Vector3 target)
        {
            Vector3 shoulder = upper.position;
            Vector3 direction = target - shoulder;
            float upperLength = Vector3.Distance(shoulder, lower.position);
            float lowerLength = Vector3.Distance(lower.position, hand.position);
            if (direction.sqrMagnitude < .0001f || upperLength < .001f || lowerLength < .001f) return;

            direction.Normalize();
            float distance = Mathf.Clamp(Vector3.Distance(shoulder, target),
                Mathf.Abs(upperLength - lowerLength) + .001f, upperLength + lowerLength - .001f);
            Vector3 pole = Vector3.ProjectOnPlane(elbowSide - Vector3.up, direction).normalized;
            float along = (upperLength * upperLength + distance * distance - lowerLength * lowerLength) / (2f * distance);
            Vector3 elbow = shoulder + direction * along + pole * Mathf.Sqrt(Mathf.Max(0f, upperLength * upperLength - along * along));
            Quaternion wrist = hand.rotation;
            upper.rotation = Quaternion.FromToRotation(lower.position - shoulder, elbow - shoulder) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(hand.position - lower.position, shoulder + direction * distance - lower.position) * lower.rotation;
            hand.rotation = wrist;
        }

        void Attach(Transform grip)
        {
            bottle.SetParent(grip, true);
            bottle.localPosition = Vector3.zero;
        }
    }
}

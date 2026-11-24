using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

namespace BullyingGame.LevelOne
{
    public sealed class LevelOneIncidentProps : MonoBehaviour
    {
        [SerializeField] private Transform lunchBox, bottle, pickupSpot, bottleRecovery;
        [SerializeField] private Transform bimaLunchGrip, bimaBottleGrip;
        [SerializeField] private Animator bima, memet;
        [SerializeField] private NavMeshAgent memetAgent, rakaAgent;
        [SerializeField] private Transform memetBehindCanteen, rakaBehindCanteen;
        [SerializeField] private double transferTime = 27.85;
        [SerializeField, Min(.01f)] private float transferDuration = .35f;

        private enum Stage { Desk, Reaching, Carrying, Timeline, AfterIncident }
        private Stage stage;
        private Arm bimaRight, bimaLeft, memetRight, shotBimaRight, shotBimaLeft, shotMemet, shotRaka;
        private PlayableDirector timeline;
        private Renderer[] hiddenGameplayProps;
        private Transform lunchHome, bottleHome;
        private Quaternion lunchRotation, bottleRotation;
        private Vector3 lunchDesk, bottleDesk, lunchCenter, bottleCenter;
        private float reach;

        public Vector3 PickupPosition => pickupSpot.position;

        private void Awake()
        {
            bimaRight = new Arm(bima, true);
            bimaLeft = new Arm(bima, false);
            memetRight = new Arm(memet, true);
            lunchHome = lunchBox.parent;
            bottleHome = bottle.parent;
            lunchDesk = lunchBox.position;
            bottleDesk = bottle.position;
            lunchRotation = lunchBox.rotation;
            bottleRotation = bottle.rotation;
            lunchCenter = MeshCenterOffset(lunchBox);
            bottleCenter = MeshCenterOffset(bottle);
        }

        private static Vector3 MeshCenterOffset(Transform prop)
        {
            var renderer = prop.GetComponentInChildren<Renderer>();
            return renderer != null ? prop.InverseTransformPoint(renderer.bounds.center) : Vector3.zero;
        }

        public IEnumerator PickUp()
        {
            var actor = bima.transform;
            Vector3 facing = Vector3.ProjectOnPlane((lunchDesk + bottleDesk) * .5f - actor.position, Vector3.up);
            if (facing.sqrMagnitude > .001f)
            {
                Quaternion start = actor.rotation;
                Quaternion end = Quaternion.LookRotation(facing);
                for (float time = 0f; time < .22f; time += Time.deltaTime)
                {
                    actor.rotation = Quaternion.Slerp(start, end, time / .22f);
                    yield return null;
                }
                actor.rotation = end;
            }
            stage = Stage.Reaching;
            for (float time = 0f; time < .85f; time += Time.deltaTime)
            {
                reach = Mathf.Clamp01(time / .85f);
                yield return null;
            }
            Carry();
        }

        public void Carry()
        {
            RestoreGameplayProps();
            stage = Stage.Carrying;
        }

        public void PlayTimeline(PlayableDirector director)
        {
            lunchBox.SetParent(lunchHome, true);
            bottle.SetParent(bottleHome, true);
            timeline = director;
            foreach (var output in director.playableAsset.outputs)
            {
                if (!(output.sourceObject is BekalPropsTrack) ||
                    !(director.GetGenericBinding(output.sourceObject) is BekalCutsceneProps props) || !props.IsConfigured) continue;

                hiddenGameplayProps = lunchBox.GetComponentsInChildren<Renderer>(true)
                    .Concat(bottle.GetComponentsInChildren<Renderer>(true))
                    .Where(renderer => !renderer.forceRenderingOff).ToArray();
                foreach (var renderer in hiddenGameplayProps) renderer.forceRenderingOff = true;
                stage = Stage.Timeline;
                return;
            }
            foreach (var output in director.playableAsset.outputs)
            {
                if (!(director.GetGenericBinding(output.sourceObject) is Animator actor) || !actor.isHuman) continue;
                switch (actor.name)
                {
                    case "Bima_Guide":
                        shotBimaRight = new Arm(actor, true);
                        shotBimaLeft = new Arm(actor, false);
                        break;
                    case "Memet": shotMemet = new Arm(actor, true); break;
                    case "Raka": shotRaka = new Arm(actor, true); break;
                }
            }
            stage = Stage.Timeline;
        }

        public void AfterIncident(bool bottleTaken)
        {
            RestoreGameplayProps();
            timeline = null;
            stage = Stage.AfterIncident;
            PlaceAtExit(memetAgent, memetBehindCanteen);
            PlaceAtExit(rakaAgent, rakaBehindCanteen);
            if (!bottleTaken)
            {
                bottle.SetParent(bottleHome, true);
                bottle.SetPositionAndRotation(bottleRecovery.position, bottleRecovery.rotation);
            }
        }

        public NavMeshAgent Pursuer => memetAgent;
        public Transform Raka => rakaAgent.transform;
        public void PlaceLunch(Vector3 surface, bool held)
        {
            stage = Stage.Desk;
            lunchBox.SetParent(lunchHome, true);
            lunchBox.rotation = lunchRotation;
            lunchBox.position = surface;
            var renderer = lunchBox.GetComponentInChildren<Renderer>(true);
            if (renderer != null) lunchBox.position += Vector3.up * (surface.y - renderer.bounds.min.y);
            lunchBox.gameObject.SetActive(!held);
        }

        public void ReturnLunch()
        {
            lunchBox.gameObject.SetActive(true);
            lunchBox.SetParent(bimaLunchGrip, false);
            lunchBox.localPosition = Vector3.zero;
        }

        private void RestoreGameplayProps()
        {
            if (hiddenGameplayProps == null) return;
            foreach (var renderer in hiddenGameplayProps)
                if (renderer != null) renderer.forceRenderingOff = false;
            hiddenGameplayProps = null;
        }

        private static void PlaceAtExit(NavMeshAgent actor, Transform destination)
        {
            if (actor == null || destination == null) return;

            if (actor.isActiveAndEnabled && actor.Warp(destination.position))
                actor.ResetPath();
            else
                actor.transform.position = destination.position;
            actor.transform.rotation = destination.rotation;
        }

        private void LateUpdate()
        {
            switch (stage)
            {
                case Stage.Reaching:
                    ReachFor(bimaRight, lunchBox, lunchDesk, lunchCenter, lunchRotation);
                    ReachFor(bimaLeft, bottle, bottleDesk, bottleCenter, bottleRotation);
                    break;
                case Stage.Carrying:
                    bimaRight.Pose(bimaRight.HoldPosition);
                    bimaLeft.Pose(bimaLeft.HoldPosition);
                    PositionAtGrip(lunchBox, lunchCenter, lunchRotation, bimaRight, bimaLunchGrip);
                    PositionAtGrip(bottle, bottleCenter, bottleRotation, bimaLeft, bimaBottleGrip);
                    break;
                case Stage.Timeline:
                    if (hiddenGameplayProps != null) break;
                    float transfer = Mathf.SmoothStep(0f, 1f,
                        Mathf.Clamp01((float)(timeline.time - transferTime) / transferDuration));
                    PositionDuringTransfer(lunchBox, lunchCenter, lunchRotation, bimaLunchGrip, shotBimaRight, shotMemet, transfer);
                    PositionDuringTransfer(bottle, bottleCenter, bottleRotation, bimaBottleGrip, shotBimaLeft, shotRaka, transfer);
                    break;
                case Stage.AfterIncident:
                    memetRight.Pose(memetRight.HoldPosition);
                    PositionAtHand(lunchBox, lunchCenter, lunchRotation, memetRight, memetRight, 0f);
                    break;
            }
        }

        private static void PositionAtGrip(Transform prop, Vector3 center, Quaternion rotation, Arm arm, Transform grip)
        {
            if (grip == null)
            {
                PositionAtHand(prop, center, rotation, arm, arm, 0f);
                return;
            }

            if (prop.parent != grip) prop.SetParent(grip, true);
            prop.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private static void PositionDuringTransfer(Transform prop, Vector3 center, Quaternion rotation,
            Transform grip, Arm from, Arm to, float blend)
        {
            if (grip == null || !from.Valid)
            {
                PositionAtHand(prop, center, rotation, from, to, blend);
                return;
            }

            var source = from.Hand.TransformPoint(grip.localPosition);
            var sourceRotation = from.Hand.rotation * grip.localRotation;
            if (!to.Valid)
            {
                prop.SetPositionAndRotation(source, sourceRotation);
                return;
            }

            prop.rotation = rotation;
            var destination = to.Hand.position - prop.TransformVector(center);
            prop.SetPositionAndRotation(Vector3.Lerp(source, destination, blend),
                Quaternion.Slerp(sourceRotation, rotation, blend));
        }

        private void ReachFor(Arm arm, Transform prop, Vector3 desk, Vector3 center, Quaternion rotation)
        {
            if (!arm.Valid) return;
            float lift = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.45f, 1f, reach));
            var start = desk + prop.TransformVector(center);
            var destination = Vector3.Lerp(start, arm.HoldPosition, lift);
            arm.Pose(Vector3.Lerp(arm.Hand.position, destination,
                Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(reach / .45f))));
            if (lift > 0f)
            {
                prop.rotation = rotation;
                prop.position = Vector3.Lerp(desk, arm.Hand.position - prop.TransformVector(center), lift);
            }
        }

        private static void PositionAtHand(Transform prop, Vector3 center, Quaternion rotation, Arm from, Arm to, float blend)
        {
            if (!from.Valid) from = to;
            if (!to.Valid) to = from;
            if (!from.Valid) return;
            prop.rotation = rotation;
            prop.position = Vector3.Lerp(from.Hand.position, to.Hand.position, blend) - prop.TransformVector(center);
        }

        private readonly struct Arm
        {
            private readonly Transform actor, upper, lower;
            private readonly float side;
            public readonly Transform Hand;
            public bool Valid => Hand != null && upper != null && lower != null;
            public Vector3 HoldPosition => !Valid ? Vector3.zero :
                upper.position + actor.forward * .21f - Vector3.up * .24f + actor.right * (side * .07f);

            public Arm(Animator animator, bool right)
            {
                side = right ? 1f : -1f;
                bool human = animator != null && animator.isHuman;
                if (human && !animator.hasTransformHierarchy)
                    AnimatorUtility.DeoptimizeTransformHierarchy(animator.gameObject);
                actor = human ? animator.transform : null;
                upper = human ? animator.GetBoneTransform(right ? HumanBodyBones.RightUpperArm : HumanBodyBones.LeftUpperArm) : null;
                lower = human ? animator.GetBoneTransform(right ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm) : null;
                Hand = human ? animator.GetBoneTransform(right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand) : null;
            }

            public void Pose(Vector3 target)
            {
                if (Valid) LevelOneBottleGrab.ReachArm(upper, lower, Hand, actor.right * side, target);
            }
        }
    }
}

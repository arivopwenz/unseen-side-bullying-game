using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using Unity.Cinemachine;

namespace BullyingGame.LevelOne
{
    public class LevelOneStaging : MonoBehaviour
    {
        [Header("Actors and freely movable stage markers")]
        [SerializeField] private Transform bima, memet, raka, teacher;
        [SerializeField] private Transform corridorBima, corridorMemet, corridorRaka, exit;
        [SerializeField] private Transform canteenBima, canteenMemet, benchBima;
        [SerializeField] private Transform classroomBima;
        [SerializeField] private CinemachineCamera storyCamera;
        public void Apply(LevelOneDirector director)
        {
            int step = director.StepIndex;
            if (step == 2) { Place(bima, corridorBima); Place(memet, corridorMemet); Place(raka, corridorRaka); }
            if (step == 3) { Place(bima, corridorBima); Place(memet, exit); Place(raka, exit); }
            if (step == 4 || step == 5) Place(bima, classroomBima);
            if (step == 6) { Place(bima, canteenBima); Place(memet, canteenMemet); }
            if (step == 7) { Place(memet, exit); Place(raka, exit); }
            if (step >= 7 && step <= 9) Place(bima, canteenBima);
            if (step >= 10) Place(bima, benchBima);
            memet.gameObject.SetActive(step == 2 || step == 6);
            raka.gameObject.SetActive(step == 2);
        }
        public bool BimaNear(Transform point) => bima != null && Vector3.Distance(bima.position, point.position) <= 6;
        public void Approach(Action finished)
        {
            StartCoroutine(Move(false, finished));
        }
        public void Depart(Action finished)
        {
            StartCoroutine(Move(true, finished));
        }
        IEnumerator Move(bool departing, Action finished)
        {
            if (memet == null || !memet.gameObject.activeInHierarchy) { finished?.Invoke(); yield break; }
            if (storyCamera != null)
            {
                Vector3 focus = (bima.position + memet.position) * .5f + Vector3.up;
                storyCamera.transform.position = focus + new Vector3(3, 2, -5);
                storyCamera.transform.rotation = Quaternion.LookRotation(focus - storyCamera.transform.position);
                storyCamera.Priority = 40;
            }
            Vector3 start = memet.position;
            Vector3 end = departing ? start + Vector3.left * 3 : start + (bima.position - start).normalized * .6f;
            var animator = memet.GetComponentInChildren<Animator>();
            bool speed = animator != null && Array.Exists(animator.parameters, p => p.name == "Speed");
            for (float time = 0; time < 1.2f; time += Time.unscaledDeltaTime)
            {
                memet.position = Vector3.Lerp(start, end, time / 1.2f);
                if (speed) animator.SetFloat("Speed", 1);
                yield return null;
            }
            if (speed) animator.SetFloat("Speed", 0);
            if (departing)
            {
                memet.gameObject.SetActive(false); raka.gameObject.SetActive(false);
                if (storyCamera != null) storyCamera.Priority = 0;
            }
            finished?.Invoke();
        }
        static void Place(Transform actor, Transform target)
        {
            if (actor == null || target == null) return;
            var agent = actor.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.Warp(target.position);
            else actor.position = target.position;
            actor.rotation = target.rotation;
        }
    }
}

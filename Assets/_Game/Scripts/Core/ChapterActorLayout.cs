using UnityEngine;
using UnityEngine.AI;

namespace BullyingGame.Core
{
    [System.Serializable]
    public struct ChapterActorPose
    {
        [SerializeField] private int chapter;
        [SerializeField] private Transform actor;
        [SerializeField] private Vector3 position;
        [SerializeField] private float yaw;
        public int Chapter => chapter;
        public Transform Actor => actor;
        public Vector3 Position => position;
        public float Yaw => yaw;
    }
    public class ChapterActorLayout : MonoBehaviour
    {
        [Header("Authored Story Locations")]
        [SerializeField] private ChapterActorPose[] poses;
        public void Apply(int chapter)
        {
            if(poses == null) return;
            foreach(var pose in poses)
            {
                if(pose.Chapter != chapter || pose.Actor == null) continue;
                var agent=pose.Actor.GetComponent<NavMeshAgent>();
                if(agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) { agent.ResetPath(); agent.Warp(pose.Position); }
                else pose.Actor.position=pose.Position;
                pose.Actor.rotation=Quaternion.Euler(0,pose.Yaw,0);
            }
            Physics.SyncTransforms();
        }
    }
}

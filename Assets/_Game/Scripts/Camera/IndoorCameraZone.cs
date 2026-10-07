using UnityEngine;
using Unity.Cinemachine;
using BullyingGame.Core;

namespace BullyingGame.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class IndoorCameraZone : MonoBehaviour
    {
        [Header("Room Bounds In World Space")]
        [SerializeField] private Transform player;
        [SerializeField] private Bounds room;
        private CinemachineCamera cameraRig;
        private void Awake() => cameraRig=GetComponent<CinemachineCamera>();
        private void LateUpdate()
        {
            bool active=player != null && room.Contains(player.position+Vector3.up) && GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Playing;
            cameraRig.Priority=active ? 12 : 0;
            if(!active) return;
            var position=player.position+Vector3.back*3.6f+Vector3.right*1.5f+Vector3.up*2.3f;
            position.x=Mathf.Clamp(position.x,room.min.x+.5f,room.max.x-.5f);
            position.z=Mathf.Clamp(position.z,room.min.z+.5f,room.max.z-.5f);
            position.y=2.35f;
            transform.SetPositionAndRotation(position,Quaternion.LookRotation(player.position+Vector3.up*1.15f-position));
        }
    }
}

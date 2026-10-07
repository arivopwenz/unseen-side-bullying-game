using UnityEngine;
using BullyingGame.Player;

namespace BullyingGame.Core
{
    public class GameplayModalGate : MonoBehaviour
    {
        [Header("Gameplay Controls")]
        [SerializeField] private Behaviour[] controls;
        private GameStateManager states;
        private bool held;
        private bool[] enabledStates;
        private CursorLockMode cursor;
        private bool cursorVisible;
        private void Start()
        {
            states=GameStateManager.Instance;
            if(states!=null) { states.OnStateChanged+=Changed;Changed(states.CurrentState,states.CurrentState); }
        }
        private void Changed(GameState previous,GameState current)
        {
            bool modal=current==GameState.Quiz || current==GameState.Result || current==GameState.Paused;
            if(modal && !held)
            {
                held=true;cursor=Cursor.lockState;cursorVisible=Cursor.visible;
                enabledStates=new bool[controls==null ? 0 : controls.Length];
                for(int i=0;i<enabledStates.Length;i++)
                {
                    if(controls[i]==null) continue;
                    enabledStates[i]=controls[i].enabled;
                    if(controls[i] is PlayerMovement movement) { movement.SetMoveInput(Vector2.zero);movement.SetSprintInput(false); }
                    controls[i].enabled=false;
                }
                Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            }
            else if(!modal) Release();
        }
        private void Release()
        {
            if(!held) return;
            for(int i=0;i<enabledStates.Length;i++) if(controls[i]!=null) controls[i].enabled=enabledStates[i];
            Cursor.lockState=cursor;Cursor.visible=cursorVisible;held=false;
        }
        private void OnDisable() { if(states!=null) states.OnStateChanged-=Changed; Release(); }
    }
}

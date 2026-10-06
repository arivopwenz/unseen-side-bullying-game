using System;
using UnityEngine;

namespace BullyingGame.Core
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }
        
        public event Action<GameState, GameState> OnStateChanged;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }


            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentState = GameState.Boot;
            Debug.Log($"Game State initialized: {CurrentState}");
        }

        public void SetState(GameState newState)
        {
            if (newState == CurrentState) return;

            GameState previousState = CurrentState;
            CurrentState = newState;
            Debug.Log($"Game State: {previousState} -> {newState}");
            OnStateChanged?.Invoke(previousState, newState);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

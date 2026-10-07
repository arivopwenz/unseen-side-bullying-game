using System;
using UnityEngine;

namespace BullyingGame.Core
{
    // Compatibility facade for old scene references: global state lives in GameStateManager.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public event Action<GameState> OnGameStateChanged;
        public GameState CurrentState => GameStateManager.Instance!=null ? GameStateManager.Instance.CurrentState : GameState.Boot;
        private GameStateManager states;
        private void Awake()
        {
            if(Instance!=null && Instance!=this) { Destroy(gameObject);return; }
            Instance=this;DontDestroyOnLoad(gameObject);
            if(GameStateManager.Instance==null) new GameObject("GameStateManager").AddComponent<GameStateManager>();
            states=GameStateManager.Instance;
            states.OnStateChanged+=Changed;
        }
        private void Changed(GameState previous,GameState current) => OnGameStateChanged?.Invoke(current);
        public void SetState(GameState state) => GameStateManager.Instance?.SetState(state);
        public bool IsPlaying() => CurrentState==GameState.Playing;
        private void OnDestroy()
        {
            if(states!=null) states.OnStateChanged-=Changed;
            if(Instance==this) Instance=null;
        }
    }
}

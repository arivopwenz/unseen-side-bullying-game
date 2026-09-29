using System;
using UnityEngine;

namespace BullyingGame.Events
{
    public class BullyingEventManager : MonoBehaviour
    {
        public static BullyingEventManager Instance { get; private set; }

        public event Action<BullyingEventData> OnEventTriggered;
        public event Action<BullyingEventData, BullyingEventState> OnEventStateChanged;

        public BullyingEventData CurrentEvent { get; private set; }
        public BullyingEventState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void TriggerEvent(BullyingEventData eventData)
        {
            if (CurrentState != BullyingEventState.Inactive) return;
            if (eventData == null) return;

            CurrentEvent = eventData;
            SetState(BullyingEventState.Triggered);
            OnEventTriggered?.Invoke(eventData);
        }

        public void StartEvent()
        {
            if (CurrentState != BullyingEventState.Triggered) return;
            SetState(BullyingEventState.InProgress);
        }

        public void WaitForResponse()
        {
            if (CurrentState != BullyingEventState.InProgress) return;
            SetState(BullyingEventState.WaitingResponse);
        }

        public void ResolveEvent()
        {
            if (CurrentEvent == null) return;
            SetState(BullyingEventState.Resolved);
            CompleteEvent();
        }

        public void FailEvent()
        {
            if (CurrentEvent == null) return;
            SetState(BullyingEventState.Failed);
            CompleteEvent();
        }

        private void SetState(BullyingEventState newState)
        {
            CurrentState = newState;
            OnEventStateChanged?.Invoke(CurrentEvent, newState);
        }

        private void CompleteEvent()
        {
            CurrentEvent = null;
            SetState(BullyingEventState.Inactive);
        }
    }
}

using System;
using System.Collections.Generic;
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
        public BullyingEventFailureReason LastFailureReason { get; private set; }
        private readonly HashSet<string> handledEvents = new HashSet<string>();
        private readonly Dictionary<BullyingEventData, BullyingEventDirector> directors =
            new Dictionary<BullyingEventData, BullyingEventDirector>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool HasHandled(BullyingEventData data)
        {
            return data != null && handledEvents.Contains(data.eventId);
        }

        public string[] CaptureHandledEvents()
        {
            var ids = new string[handledEvents.Count]; handledEvents.CopyTo(ids); return ids;
        }

        public void RestoreHandledEvents(string[] ids)
        {
            if (CurrentEvent != null) return;
            handledEvents.Clear();
            if (ids != null) foreach (var id in ids)
                if (!string.IsNullOrWhiteSpace(id)) handledEvents.Add(id);
        }

        public bool RegisterDirector(BullyingEventData data, BullyingEventDirector director)
        {
            if (data == null || director == null || string.IsNullOrWhiteSpace(data.eventId))
                return false;
            foreach (var registration in directors)
            {
                if (registration.Key.eventId == data.eventId && registration.Value != director)
                {
                    Debug.LogError($"Duplicate bullying event ID: {data.eventId}", director);
                    return false;
                }
            }
            directors[data] = director;
            return true;
        }

        public void UnregisterDirector(BullyingEventData data, BullyingEventDirector director)
        {
            if (data != null && directors.TryGetValue(data, out var registered) && registered == director)
                directors.Remove(data);
        }

        public bool TryTriggerEvent(BullyingEventData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.eventId) ||
                !isActiveAndEnabled || CurrentState != BullyingEventState.Inactive || HasHandled(data) ||
                !directors.TryGetValue(data, out var director) || director == null ||
                !director.CanStart(data)) return false;
            CurrentEvent = data;
            LastFailureReason = BullyingEventFailureReason.Interrupted;
            SetState(BullyingEventState.Triggered);
            OnEventTriggered?.Invoke(data);
            return true;
        }

        public void TriggerEvent(BullyingEventData data) => TryTriggerEvent(data);

        public void StartEvent()
        {
            if (CurrentState == BullyingEventState.Triggered)
                SetState(BullyingEventState.InProgress);
        }

        public void WaitForResponse()
        {
            if (CurrentState == BullyingEventState.InProgress || CurrentState == BullyingEventState.Confrontation)
                SetState(BullyingEventState.WaitingResponse);
        }

        public void StartApproach()
        {
            if (CurrentState == BullyingEventState.InProgress)
                SetState(BullyingEventState.Approaching);
        }

        public void StartConfrontation()
        {
            if (CurrentState == BullyingEventState.InProgress || CurrentState == BullyingEventState.Approaching)
                SetState(BullyingEventState.Confrontation);
        }

        public void ResolveEvent() => Finish(BullyingEventState.Resolved);
        public void FailEvent() => FailEvent(BullyingEventFailureReason.Interrupted);
        public void FailEvent(BullyingEventFailureReason reason) =>
            Finish(BullyingEventState.Failed, reason);

        private void Finish(BullyingEventState result,
            BullyingEventFailureReason reason = BullyingEventFailureReason.Interrupted)
        {
            if (CurrentEvent == null ||
                (CurrentState != BullyingEventState.Triggered &&
                 CurrentState != BullyingEventState.InProgress &&
                 CurrentState != BullyingEventState.Approaching &&
                 CurrentState != BullyingEventState.Confrontation &&
                 CurrentState != BullyingEventState.WaitingResponse)) return;
            handledEvents.Add(CurrentEvent.eventId);
            LastFailureReason = reason;
            SetState(result);
            CurrentEvent = null;
            SetState(BullyingEventState.Inactive);
        }

        private void SetState(BullyingEventState state)
        {
            CurrentState = state;
            OnEventStateChanged?.Invoke(CurrentEvent, state);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

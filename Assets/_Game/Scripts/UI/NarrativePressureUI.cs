using UnityEngine;
using UnityEngine.UI;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.Events;

namespace BullyingGame.UI
{
    public class NarrativePressureUI : MonoBehaviour
    {
        [Header("Pressure: Steady, No Flashing")]
        [SerializeField] private Image vignette;
        [SerializeField] private AudioSource heartbeat;
        [SerializeField] private DialogueData[] pressureDialogues;
        [SerializeField] private NarrativeActivityDirector activity;
        private float strength;
        private void Update()
        {
            if(vignette == null) return;
            bool pressured=activity != null && activity.IsPressureActive;
            var dialogue=DialogueManager.Instance;
            if(dialogue != null && dialogue.IsDialogueActive && pressureDialogues != null)
                foreach(var data in pressureDialogues) pressured |= data == dialogue.CurrentDialogue;
            bool paused=GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.Paused;
            if(!paused) strength=Mathf.MoveTowards(strength, pressured ? (PlayerAccessibility.ReducedPressure ? 0 : .58f) : 0, Time.unscaledDeltaTime*.6f);
            vignette.color=new Color(.15f,.04f,.06f,strength);
            if(heartbeat != null)
            {
                heartbeat.volume=paused ? 0 : strength*.13f;
                if(strength>.01f && !paused && !heartbeat.isPlaying) heartbeat.Play();
                if((strength<=.01f || paused) && heartbeat.isPlaying) heartbeat.Stop();
            }
        }
        private void OnDisable() { if(heartbeat != null) heartbeat.Stop(); if(vignette != null) vignette.color=Color.clear; }
    }
}

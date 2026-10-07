using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace BullyingGame.Dialogue
{
    public class DialogueChoiceUI : MonoBehaviour
    {
        [Header("Aji Response Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] buttons;
        [SerializeField] private TMP_Text[] labels;
        private DialogueManager manager;
        private GameObject previousSelection;
        private void Start()
        {
            if(panel != null) panel.SetActive(false);
            manager = DialogueManager.Instance;
            if(manager == null) return;
            manager.OnChoicesRequested += Show;
            manager.OnLineDisplayed += Line;
            manager.OnDialogueEnded += Hide;
            for(int i=0;i<buttons.Length;i++) { int index=i; buttons[i].onClick.AddListener(() => Choose(index)); }
        }
        private void Show(DialogueResponse[] choices)
        {
            if(panel == null || choices == null || choices.Length != buttons.Length) return;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            panel.SetActive(true);
            for(int i=0;i<buttons.Length;i++) { labels[i].text = choices[i].Label; buttons[i].interactable=true; }
            if(EventSystem.current != null) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
        public void Choose(int index) { if(manager != null && manager.SelectChoice(index)) Hide(); }
        private void Line(DialogueLine line) { if(manager != null && !manager.IsAwaitingChoice) Hide(); }
        private void Hide()
        {
            if(panel == null || !panel.activeSelf) return;
            panel.SetActive(false);
            if(EventSystem.current != null) EventSystem.current.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
            previousSelection=null;
        }
        private void OnDestroy()
        {
            if(manager != null) { manager.OnChoicesRequested -= Show; manager.OnLineDisplayed -= Line; manager.OnDialogueEnded -= Hide; }
        }
    }
}

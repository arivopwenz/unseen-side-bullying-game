using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BullyingGame.UI;

namespace BullyingGame.LevelOne
{
    public class LevelOneView : MonoBehaviour
    {
        RectTransform canvas, modal, hud;
        TMP_Text title, objective, prompt, cardText, navigation, saveText;
        RectTransform promptBackdrop;
        [Header("Keep the project's existing quest HUD")]
        [SerializeField] private bool useExistingHUD;
        RectTransform trackerPanel;
        WordCardData puzzle;
        readonly List<string> selected = new List<string>();
        int[] failures;
        Action solved, save;
        string puzzleFeedback = "Klik keping berurutan untuk mengisi tiga slot. Klik slot untuk melepas keping.";
        void Awake()
        {
            var root = new GameObject("Level01 PRD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false); canvas = root.GetComponent<RectTransform>();
            var c = root.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 200;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            hud = StoryOverlay.Rect(canvas, "Gameplay HUD", Vector2.zero, Vector2.zero); hud.anchorMin = Vector2.zero; hud.anchorMax = Vector2.one; hud.sizeDelta = Vector2.zero;
            var tracker = Panel(hud, "Quest tracker", new Vector2(-650, 410), new Vector2(560, 190));
            trackerPanel = tracker;
            title = Text(tracker, "Title", "", new Vector2(0, 53), new Vector2(520, 50), 28);
            objective = Text(tracker, "Objective", "", new Vector2(0, -20), new Vector2(520, 100), 25);
            promptBackdrop = Panel(hud, "Prompt backdrop", new Vector2(0, -260), new Vector2(1120, 90));
            prompt = Text(promptBackdrop, "Interaction prompt", "", Vector2.zero, new Vector2(1080, 80), 29); prompt.alignment = TextAlignmentOptions.Center;
            var cards = Panel(hud, "Kartu Aksi", new Vector2(620, -410), new Vector2(610, 160));
            Text(cards, "Label", "KARTU AKSI", new Vector2(0, 48), new Vector2(570, 35), 23);
            cardText = Text(cards, "Cards", "", new Vector2(0, -15), new Vector2(570, 85), 25);
            navigation = Text(hud, "Destination", "", new Vector2(0, 430), new Vector2(660, 90), 24); navigation.alignment = TextAlignmentOptions.Center;
            saveText = Text(hud, "Save and controls", "", new Vector2(-560, -480), new Vector2(720, 65), 21);
        }
        public void Refresh(LevelOneDirector director, string[] cards, int active)
        {
            hud.gameObject.SetActive(!director.IsModal);
            title.text = director.QuestTitle; objective.text = director.Objective;
            trackerPanel.gameObject.SetActive(!useExistingHUD);
            navigation.gameObject.SetActive(!useExistingHUD);
            prompt.text = director.Nearest == null ? "" : "E • " + director.Nearest.GetPromptText();
            promptBackdrop.gameObject.SetActive(director.Nearest != null);
            cardText.text = cards.Length == 0 ? "Dekati penanda tujuan untuk melihat aksi." : string.Join("    ", cards.Select((s, i) => (i == active ? "<color=#FFD46B>" : "<color=#C2D6DF>") + (i + 1) + "  " + s + "</color>"));
            saveText.text = director.SaveFailed ? "Simpan gagal. ESC → Simpan untuk mencoba lagi." : "TAB  Jurnal     ESC  Jeda     Barang " + director.HeldCount + "/2";
            var target = director.Target;
            if (target == null) navigation.text = "";
            else
            {
                float distance = Vector3.Distance(director.Player.transform.position, target.transform.position);
                Vector3 delta = target.transform.position - director.Player.transform.position;
                var camera = UnityEngine.Camera.main;
                float angle = camera != null ? Vector3.SignedAngle(Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up), Vector3.ProjectOnPlane(delta, Vector3.up), Vector3.up) : 0;
                navigation.text = (angle < -30 ? "← " : angle > 30 ? "→ " : "↑ ") + target.Label + "  •  " + Mathf.RoundToInt(distance) + " m" + (director.HintActive ? "\nPetunjuk: rak troli (P3) dan bangku pojok (P5)." : "");
            }
        }
        static TMP_Text Text(Transform p, string name, string text, Vector2 position, Vector2 size, float font) => StoryOverlay.Text(p, name, text, position, size, font);
        static RectTransform Panel(Transform p, string name, Vector2 position, Vector2 size)
        {
            var r = StoryOverlay.Rect(p, name, position, size); r.gameObject.AddComponent<Image>().color = new Color(.025f, .06f, .1f, .96f); return r;
        }
        RectTransform Modal(string name)
        {
            HideModal(); hud.gameObject.SetActive(false);
            modal = Panel(canvas, "Modal", Vector2.zero, new Vector2(4000, 3000));
            modal.GetComponent<Image>().color = new Color(.01f, .02f, .04f, .82f);
            var panel = Panel(modal, name, Vector2.zero, new Vector2(1220, 850));
            Text(panel, "Heading", name, new Vector2(0, 355), new Vector2(1120, 70), 32); return panel;
        }
        public void HideModal() { if (modal != null) { modal.gameObject.SetActive(false); Destroy(modal.gameObject); modal = null; } }
        public void ShowLines(string[] lines, Action finished)
        {
            if (lines == null || lines.Length == 0) { finished?.Invoke(); return; }
            int index = 0;
            var panel = Modal("LANGKAH PERTAMA");
            var body = Text(panel, "Dialogue", lines[0], new Vector2(0, 20), new Vector2(1090, 510), 32);
            var counter = Text(panel, "Page", "1 / " + lines.Length, new Vector2(-430, -350), new Vector2(200, 45), 24);
            Button nextButton = null;
            nextButton = StoryOverlay.Button(panel, "Lanjut", new Vector2(400, -345), new Vector2(260, 65), () =>
            {
                if (++index >= lines.Length) { nextButton.interactable = false; finished?.Invoke(); return; }
                body.text = lines[index]; counter.text = (index + 1) + " / " + lines.Length;
            });
        }
        public void ShowPuzzle(WordCardData data, int[] attempts, Action completed, Action saved)
        {
            puzzle = data; failures = attempts; solved = completed; save = saved; selected.Clear();
            puzzleFeedback = "Klik keping berurutan. Klik slot untuk melepasnya; keping salah belum diucapkan ke Bima.";
            DrawPuzzle();
        }
        void DrawPuzzle()
        {
            var panel = Modal("KARTU KATA • " + puzzle.Id + " — " + puzzle.Title);
            Text(panel, "Context", puzzle.Context, new Vector2(0, 250), new Vector2(1100, 100), 26);
            for (int i = 0; i < 3; i++)
            {
                int slot = i; string label = i < selected.Count ? puzzle.TextFor(selected[i]) : "Slot " + (i + 1);
                StoryOverlay.Button(panel, label, new Vector2((i - 1) * 360, 130), new Vector2(345, 95), () => { if (slot < selected.Count) { selected.RemoveAt(slot); DrawPuzzle(); } });
            }
            // Stable, shuffled presentation; correct phrases are never preselected.
            var phrases = puzzle.Phrases.OrderBy(p => p.id).ToArray();
            for (int i = 0; i < phrases.Length; i++)
            {
                var phrase = phrases[i];
                var b = StoryOverlay.Button(panel, phrase.text, new Vector2((i % 3 - 1) * 360, -i / 3 * 95), new Vector2(345, 80), () => { if (selected.Count < 3 && !selected.Contains(phrase.id)) { selected.Add(phrase.id); DrawPuzzle(); } });
                b.interactable = !selected.Contains(phrase.id);
            }
            int failureIndex = puzzle.Id == "K-01" ? 0 : puzzle.Id == "K-02" ? 1 : 2;
            string feedback = puzzleFeedback + (failures[failureIndex] >= 2 ? "\nPetunjuk: " + puzzle.Hint : "");
            Text(panel, "Feedback", feedback, new Vector2(0, -205), new Vector2(1100, 135), 24);
            StoryOverlay.Button(panel, "Susun ulang", new Vector2(-400, -345), new Vector2(260, 65), () => { selected.Clear(); DrawPuzzle(); });
            if (failures[failureIndex] >= 4) StoryOverlay.Button(panel, "Bantu susun", new Vector2(0, -345), new Vector2(260, 65), () => { selected.Clear(); selected.AddRange(puzzle.Answer); DrawPuzzle(); });
            StoryOverlay.Button(panel, "Ucapkan", new Vector2(400, -345), new Vector2(260, 65), () =>
            {
                if (selected.Count != 3) { puzzleFeedback = "Isi ketiga slot dahulu."; DrawPuzzle(); return; }
                if (!puzzle.IsCorrect(selected))
                {
                    failures[failureIndex]++; save?.Invoke(); puzzleFeedback = puzzle.Feedback; DrawPuzzle();
                    Debug.Log("[Level01 PRD] " + puzzle.Id + " retry " + failures[failureIndex]); return;
                }
                Debug.Log("[Level01 PRD] " + puzzle.Id + " solved, retries=" + failures[failureIndex]);
                ShowLines(new[] { "Aji: “" + string.Join(" ", selected.Select(puzzle.TextFor)) + "”", puzzle.Response, "Mengapa aman: " + puzzle.Hint }, solved);
            });
        }
        public void ShowJournal(string body, Action close)
        {
            var panel = Modal("JURNAL • TUJUAN, PETUNJUK & PEMBELAJARAN");
            var viewport = StoryOverlay.Rect(panel, "Viewport", new Vector2(0, 5), new Vector2(1100, 590));
            viewport.gameObject.AddComponent<Image>().color = Color.clear; viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            var text = Text(viewport, "Entries", string.IsNullOrEmpty(body) ? "Catatan akan muncul saat Aji memahami kejadian." : body, Vector2.zero, new Vector2(1060, 590), 26);
            var rect = text.rectTransform; rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(-30, 590);
            text.ForceMeshUpdate(); rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(590, text.preferredHeight + 30));
            scroll.viewport = viewport; scroll.content = rect;
            StoryOverlay.Button(panel, "Kembali", new Vector2(400, -345), new Vector2(260, 65), close);
        }
        public void ShowPause(Action close, Action saveAction, Action restart)
        {
            var p = Modal("JEDA");
            Text(p, "Controls", "WASD — berjalan\nMouse — kamera\n1–3 — pilih Kartu Aksi\nE — jalankan aksi di dekat penanda\nTAB — jurnal\n\nKartu Kata: klik frasa berurutan; klik slot untuk membatalkan. Tidak ada timer atau penalti.\nProgres otomatis tersimpan setelah setiap langkah dan barang.", new Vector2(0, 40), new Vector2(1100, 500), 28);
            StoryOverlay.Button(p, "Lanjut", new Vector2(-380, -345), new Vector2(260, 65), close);
            StoryOverlay.Button(p, "Simpan", new Vector2(0, -345), new Vector2(260, 65), saveAction);
            StoryOverlay.Button(p, "Ulang Level 1", new Vector2(380, -345), new Vector2(260, 65), restart);
        }
        public void ConfirmRestart(Action yes, Action no)
        {
            var p = Modal("ULANG LEVEL 1?");
            Text(p, "Warning", "Progres Kartu Aksi dan Kartu Kata Level 1 akan dimulai ulang.", Vector2.zero, new Vector2(1100, 200), 30);
            StoryOverlay.Button(p, "Batal", new Vector2(-220, -345), new Vector2(260, 65), no);
            StoryOverlay.Button(p, "Ulang", new Vector2(220, -345), new Vector2(260, 65), yes);
        }
    }
}

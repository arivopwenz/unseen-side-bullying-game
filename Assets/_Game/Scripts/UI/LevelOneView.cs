using System;
using System.Collections;
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
        RectTransform promptBackdrop, actionPanel;
        LevelOneSaveData dayTwoProgress;
        Action closePuzzle, cancelModal;
        ScrollRect journalScroll;
        [Header("Keep the project's existing quest HUD")]
        [SerializeField] private bool useExistingHUD;
        [Header("Kartu Aksi")]
        [SerializeField] private CanvasGroup actionCardArtwork;
        [SerializeField] private Image actionCardImage;
        [Header("Dialog")]
        [SerializeField] private TMP_FontAsset speakerFont;
        [SerializeField] private TMP_FontAsset dialogueFont;
        CanvasGroup wordCardArtwork, activeCardArtwork;
        Image wordCardImage, activeCardImage;
        Coroutine cardReveal;
        Vector3 cardRestScale;
        RectTransform trackerPanel;
        WordCardData puzzle;
        readonly List<string> selected = new List<string>();
        int[] failures;
        Action solved, save;
        string puzzleFeedback = "Klik keping berurutan untuk mengisi tiga slot. Klik slot untuk melepas keping.";
        UnityEngine.AI.NavMeshPath campaignPath;
        Transform lastDestination;
        float nextRouteUpdate;
        Vector3 waypoint;
        void Awake()
        {
            if (actionCardArtwork != null && actionCardImage != null)
            {
                actionCardArtwork.gameObject.SetActive(false);
            }

            var wordCard = transform.Find("Canvas_KartuKata");
            if (wordCard != null)
            {
                wordCardArtwork = wordCard.GetComponent<CanvasGroup>();
                // The artwork image still has the name from the duplicated action card.
                wordCardImage = wordCard.Find("Image_KartuAksi")?.GetComponent<Image>();
                wordCard.gameObject.SetActive(false);
            }

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
            var cards = actionPanel = Panel(hud, "Kartu Aksi", new Vector2(620, -410), new Vector2(610, 160));
            Text(cards, "Label", "KARTU AKSI", new Vector2(0, 48), new Vector2(570, 35), 23);
            cardText = Text(cards, "Cards", "", new Vector2(0, -15), new Vector2(570, 85), 25);
            navigation = Text(hud, "Destination", "", new Vector2(0, 430), new Vector2(660, 90), 24); navigation.alignment = TextAlignmentOptions.Center;
            saveText = Text(hud, "Save and controls", "", new Vector2(-560, -480), new Vector2(720, 65), 21);
        }
        public void Refresh(LevelOneDirector director, string[] cards, int active)
        {
            hud.gameObject.SetActive(!director.IsModal);
            actionPanel.gameObject.SetActive(!director.IsDayTwo);
            title.text = director.QuestTitle; objective.text = director.Objective;
            trackerPanel.gameObject.SetActive(!useExistingHUD);
            navigation.gameObject.SetActive(!useExistingHUD);
            var flow = director.DayTwoFlow;
            bool canAsk = flow != null && flow.CanAsk;
            prompt.text = canAsk ? FormatControl(director.InteractPrompt, "Tanya " + flow.StudentName) : director.Nearest == null ? "" : FormatControl(director.InteractPrompt, director.Nearest.GetPromptText());
            promptBackdrop.gameObject.SetActive(canAsk || director.Nearest != null);
            cardText.text = cards.Length == 0 ? "Dekati penanda tujuan untuk melihat aksi." : string.Join("    ", cards.Select((s, i) => (i == active ? "<color=#FFD46B>" : "<color=#C2D6DF>") + (i + 1) + "  " + s + "</color>"));
            saveText.text = director.SaveFailed ? "Simpan gagal. Coba simpan lagi dari menu jeda." :
                FormatControl(director.JournalPrompt, "Jurnal") + "     " + FormatControl(director.PausePrompt, "Jeda");
            var target = director.Target;
            if (target == null) navigation.text = "";
            else
            {
                var destination = flow != null && flow.InterviewStudent != null ? flow.InterviewStudent.transform : target.transform;
                string label = destination == target.transform ? target.Label : flow.StudentName;
                float distance = Vector3.Distance(director.Player.transform.position, destination.position);
                Vector3 delta = destination.position - director.Player.transform.position;
                var camera = UnityEngine.Camera.main;
                float angle = camera != null ? Vector3.SignedAngle(Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up), Vector3.ProjectOnPlane(delta, Vector3.up), Vector3.up) : 0;
                navigation.text = (angle < -30 ? "← " : angle > 30 ? "→ " : "↑ ") + label + "  •  " + Mathf.RoundToInt(distance) + " m" + (director.HintActive && !director.IsDayTwo ? "\nPetunjuk: rak troli (P3) dan bangku pojok (P5)." : "");
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
        public void HideModal()
        {
            HideCardArtwork();
            cancelModal = null;
            journalScroll = null;
            controlHelp = null;
            puzzleHelp = null;
            if (modal == null) return;
            var events = UnityEngine.EventSystems.EventSystem.current;
            var focused = events != null ? events.currentSelectedGameObject : null;
            if (focused != null && focused.transform.IsChildOf(modal))
                events.SetSelectedGameObject(null);
            modal.gameObject.SetActive(false);
            Destroy(modal.gameObject);
            modal = null;
        }
        public void CancelModal() => cancelModal?.Invoke();

        public void ScrollJournal(float delta)
        {
            if (journalScroll != null && Mathf.Abs(delta) > .001f)
                journalScroll.verticalNormalizedPosition = Mathf.Clamp01(journalScroll.verticalNormalizedPosition + delta);
        }

        public void ShowLines(string[] lines, Action finished)
            => ShowDialogue(lines, finished, false);

        public void ShowCinematicLines(string[] lines, Action finished)
            => ShowDialogue(lines, finished, true);

        void ShowDialogue(string[] lines, Action finished, bool skippable)
        {
            if (lines == null || lines.Length == 0) { finished?.Invoke(); return; }
            HideModal();
            hud.gameObject.SetActive(false);
            modal = StoryOverlay.Rect(canvas, "Dialog", Vector2.zero, Vector2.zero);
            modal.anchorMin = Vector2.zero;
            modal.anchorMax = Vector2.one;
            modal.sizeDelta = Vector2.zero;
            var panel = Panel(modal, "Percakapan", Vector2.zero, Vector2.zero);
            panel.anchorMin = new Vector2(.07f, 0);
            panel.anchorMax = new Vector2(.93f, 0);
            panel.pivot = new Vector2(.5f, 0);
            panel.sizeDelta = new Vector2(0, 230);
            panel.anchoredPosition = new Vector2(0, 24);
            var speaker = Text(panel, "Speaker", "", Vector2.zero, Vector2.zero, 31);
            var body = Text(panel, "Dialogue", "", Vector2.zero, Vector2.zero, 32);
            StretchDialogueText(speaker, 165, 53);
            StretchDialogueText(body, 65, 100);
            speaker.color = new Color32(255, 212, 111, 255);
            if (speakerFont != null) speaker.font = speakerFont;
            if (dialogueFont != null) body.font = dialogueFont;
            body.enableAutoSizing = true;
            body.fontSizeMin = 22;
            body.fontSizeMax = 32;
            body.alignment = TextAlignmentOptions.MidlineLeft;
            var counter = Text(panel, "Page", "", Vector2.zero, new Vector2(160, 44), 22);
            counter.rectTransform.anchorMin = counter.rectTransform.anchorMax = Vector2.zero;
            counter.rectTransform.anchoredPosition = new Vector2(115, 34);
            int index = 0;
            void Draw()
            {
                string line = lines[index] ?? "";
                int split = line.IndexOf(':');
                speaker.text = split > 0 && split <= 32 ? line.Substring(0, split) : "";
                body.text = speaker.text.Length > 0 ? line.Substring(split + 1).TrimStart() : line;
                counter.text = (index + 1) + " / " + lines.Length;
            }
            bool done = false;
            void Finish()
            {
                if (done) return;
                done = true;
                finished?.Invoke();
            }
            Draw();
            var nextButton = StoryOverlay.Button(panel, "Lanjut", Vector2.zero, new Vector2(180, 50), () =>
            {
                if (done) return;
                if (++index == lines.Length) Finish();
                else Draw();
            });
            AnchorDialogueButton(nextButton, -122);
            if (skippable)
                AnchorDialogueButton(StoryOverlay.Button(panel, "Lewati adegan", Vector2.zero,
                    new Vector2(215, 50), Finish), -335);
            Select(nextButton);
        }
        static void StretchDialogueText(TMP_Text text, float bottom, float height)
        {
            var rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.right;
            rect.offsetMin = new Vector2(36, bottom);
            rect.offsetMax = new Vector2(-36, bottom + height);
        }
        static void AnchorDialogueButton(Button button, float offset)
        {
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = Vector2.right;
            rect.anchoredPosition = new Vector2(offset, 34);
        }
        public void SetGameplayVisible(bool visible) => hud.gameObject.SetActive(visible);
        public void RefreshCampaign(string questTitle, string currentObjective, string interaction, string controls, bool modalOpen,
            Transform player = null, Transform destination = null)
        {
            hud.gameObject.SetActive(!modalOpen);
            actionPanel.gameObject.SetActive(false);
            trackerPanel.gameObject.SetActive(true);
            bool guide = destination != null && player != null && string.IsNullOrEmpty(interaction);
            navigation.gameObject.SetActive(guide);
            if (guide)
            {
                if (lastDestination != destination || Time.unscaledTime >= nextRouteUpdate)
                {
                    campaignPath ??= new UnityEngine.AI.NavMeshPath();
                    waypoint = MissionNavigationHint.NextWaypoint(player.position, destination.position, campaignPath);
                    lastDestination = destination;
                    nextRouteUpdate = Time.unscaledTime + .3f;
                }
                var camera = UnityEngine.Camera.main;
                var facing = camera != null ? camera.transform.forward : player.forward;
                float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(facing, Vector3.up),
                    Vector3.ProjectOnPlane(waypoint - player.position, Vector3.up), Vector3.up);
                string arrow = Mathf.Abs(angle) > 145 ? "↓" : angle < -35 ? "←" : angle > 35 ? "→" : "↑";
                navigation.text = arrow + "   " + Mathf.RoundToInt(Vector3.Distance(player.position, destination.position)) + " m";
            }
            title.text = questTitle;
            objective.text = currentObjective;
            prompt.text = interaction;
            promptBackdrop.gameObject.SetActive(!string.IsNullOrEmpty(interaction));
            saveText.text = controls;
        }

        public void ShowOptions(string heading, string body, string[] options, Action<int> picked, Action back = null)
        {
            var panel = Modal(heading);
            cancelModal = back;
            Text(panel, "Task", body, new Vector2(0, 160), new Vector2(1090, 280), 28);
            float height = Mathf.Min(74, 290f / Mathf.Max(1, options.Length));
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                var button = StoryOverlay.Button(panel, options[i], new Vector2(0, -35 - i * (height + 9)),
                    new Vector2(1080, height), () => picked(index));
                if (i == 0) Select(button);
            }
        }
        public void ShowActionCard(string objectiveText, Action finished)
        {
            ShowCardArtwork(actionCardArtwork, actionCardImage, () => ShowCard("Kartu Aksi", objectiveText, finished));
        }

        void ShowCardArtwork(CanvasGroup artwork, Image image, Action finished)
        {
            if (artwork == null || image == null || image.sprite == null)
            {
                finished();
                return;
            }

            HideModal();
            SetGameplayVisible(false);
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
            activeCardArtwork = artwork;
            activeCardImage = image;
            cardRestScale = image.rectTransform.localScale;
            artwork.alpha = 0f;
            artwork.gameObject.SetActive(true);
            cardReveal = StartCoroutine(RevealCardArtwork(finished));
        }

        IEnumerator RevealCardArtwork(Action finished)
        {
            const float popEnd = .36f;
            const float flipEnd = 1.26f;
            const float exitStart = 2f;
            const float exitEnd = 2.28f;
            float elapsed = 0f;
            while (elapsed < exitEnd)
            {
                float pop = Mathf.Clamp01(elapsed / popEnd);
                float flip = Mathf.InverseLerp(popEnd, flipEnd, elapsed);
                float exit = Mathf.InverseLerp(exitStart, exitEnd, elapsed);
                float scale = Mathf.Lerp(.58f, 1f, Mathf.SmoothStep(0f, 1f, pop))
                    + Mathf.Sin(pop * Mathf.PI) * .17f;
                scale *= Mathf.Lerp(1f, .83f, exit);
                float width = Mathf.Max(.02f, Mathf.Abs(Mathf.Cos(flip * 2f * Mathf.PI)));
                activeCardImage.rectTransform.localScale = Vector3.Scale(cardRestScale, new Vector3(scale * width, scale, 1f));
                activeCardArtwork.alpha = Mathf.Clamp01(pop * 3f) * (1f - exit);
                yield return null;
                elapsed += Mathf.Min(Time.unscaledDeltaTime, .09f);
            }

            cardReveal = null;
            HideCardArtwork();
            finished();
        }

        void HideCardArtwork()
        {
            if (cardReveal != null)
            {
                StopCoroutine(cardReveal);
                cardReveal = null;
            }
            if (activeCardArtwork != null) activeCardArtwork.gameObject.SetActive(false);
            if (activeCardImage != null) activeCardImage.rectTransform.localScale = cardRestScale;
            activeCardArtwork = null;
            activeCardImage = null;
        }

        void OnDisable() => HideCardArtwork();

        public void ShowCard(string heading, string objectiveText, Action finished)
        {
            var panel = Modal(heading.ToUpperInvariant());
            Text(panel, "Objective", objectiveText, Vector2.zero, new Vector2(1060, 260), 46);
            Select(StoryOverlay.Button(panel, "Lanjut", new Vector2(400, -345), new Vector2(260, 65), finished));
            StartCoroutine(RevealCard(panel));
        }
        static System.Collections.IEnumerator RevealCard(RectTransform panel)
        {
            float elapsed = 0;
            while (panel != null && panel.gameObject.activeInHierarchy && elapsed < .3f)
            {
                elapsed += Time.unscaledDeltaTime;
                panel.localScale = Vector3.one * Mathf.Lerp(.88f, 1f, elapsed / .3f);
                yield return null;
            }
        }
        static void Select(Button button) => UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(button.gameObject);
        public void ShowDayTwoPuzzle(WordCardData data, LevelOneSaveData progress, Action completed, Action close, Action saved)
        {
            ShowCardArtwork(wordCardArtwork, wordCardImage, () => StartCoroutine(WordAttempt(data, completed)));
        }

        RectTransform BottomPanel(float height)
        {
            HideModal();
            modal = Panel(canvas, "Percakapan", Vector2.zero, new Vector2(1230, height));
            modal.anchorMin = modal.anchorMax = new Vector2(.5f, 0);
            modal.pivot = new Vector2(.5f, 0);
            modal.anchoredPosition = new Vector2(0, 18);
            return modal;
        }

        internal void Caption(string speaker, string line)
        {
            var panel = BottomPanel(125);
            Text(panel, "Dialog", speaker + "  •  " + line, Vector2.zero, new Vector2(1160, 110), 31);
        }

        internal void ChaseCall() => StartCoroutine(ShowChaseCall());
        IEnumerator ShowChaseCall()
        {
            var call = Text(hud, "Memet", "Memet: Ji, tunggu dulu!", new Vector2(0,-200), new Vector2(800,60), 32);
            yield return new WaitForSecondsRealtime(1.4f);
            if (call != null) Destroy(call.gameObject);
        }

        IEnumerator WordAttempt(WordCardData data, Action completed)
        {
            var panel = BottomPanel(300);
            var status = Text(panel, "Instruksi", "", new Vector2(0, 124), new Vector2(1150, 45), 25);
            var bar = Panel(panel, "Sisa waktu", new Vector2(0, 98), new Vector2(1140, 9));
            bar.pivot = new Vector2(0, .5f);
            bar.anchoredPosition = new Vector2(-570, 98);
            var slots = new RectTransform[data.Answer.Length];
            var filled = new bool[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = Panel(panel, "Slot " + (i+1), new Vector2((i-1)*390,58), new Vector2(375,62));
                Text(slots[i], "Nomor", (i+1).ToString(), Vector2.zero, new Vector2(365,60), 23);
            }
            int lives = 3;
            var distractors = data.Phrases.Where(p => !data.Answer.Contains(p.id)).ToArray();
            for (int column = 0; column < slots.Length; column++)
            {
                int slot = column;
                int correctRow = UnityEngine.Random.Range(0, 2);
                for (int row = 0; row < 2; row++)
                {
                    bool correct = row == correctRow;
                    string id = correct ? data.Answer[slot] : distractors[slot].id;
                    string label = data.TextFor(id);
                    var tile = Panel(panel, id, new Vector2((slot-1)*390, -21-row*68), new Vector2(375,60));
                    tile.GetComponent<Image>().color = new Color(.12f,.32f,.35f,.97f);
                    Text(tile, "Kata", label, Vector2.zero, new Vector2(367,58), 23);
                    tile.gameObject.AddComponent<WordTileDrag>().Dropped = pointer =>
                    {
                        if (filled[slot] || !RectTransformUtility.RectangleContainsScreenPoint(slots[slot], pointer)) return;
                        if (!correct) { lives--; return; }
                        filled[slot] = true;
                        slots[slot].GetComponentInChildren<TMP_Text>().text = label;
                        tile.gameObject.SetActive(false);
                    };
                }
            }
            float remaining = 40;
            while (remaining > 0 && lives > 0 && !filled.All(x=>x) && panel != null && panel.gameObject.activeSelf)
            {
                remaining -= Time.unscaledDeltaTime;
                float fraction = Mathf.Clamp01(remaining/40f);
                bar.localScale = new Vector3(fraction,1,1);
                bar.GetComponent<Image>().color = Color.Lerp(Color.red,Color.green,fraction);
                status.text = "Seret satu jawaban ke slot tepat di atasnya   •   Nyawa " + lives + "/3";
                yield return null;
            }
            if (panel == null || !panel.gameObject.activeSelf) yield break;
            if (filled.All(x=>x))
                ShowLines(new[] { "Aji: " + string.Join(" ",data.Answer.Select(data.TextFor)), "Memet: Aku baru kepikiran gimana rasanya jadi Bima. Maaf, Ji. Botolnya di dekat pohon taman. Bekalnya di meja kelas 8B." }, completed);
            else ShowLines(new[] { lives <= 0 ? "Nyawa habis. Coba susun nasihat Aji sekali lagi." : "Waktu habis. Coba susun nasihat Aji sekali lagi." }, () => StartCoroutine(WordAttempt(data, completed)));
        }
        void RedrawPuzzle()
        {
            if (dayTwoProgress != null)
            {
                dayTwoProgress.wordSlots = selected.ToArray();
                save?.Invoke();
            }
            DrawPuzzle();
        }
        public void ShowPuzzle(WordCardData data, int[] attempts, Action completed, Action saved)
        {
            dayTwoProgress = null; puzzle = data; failures = attempts; solved = completed; save = saved; selected.Clear();
            puzzleFeedback = "Klik keping berurutan. Klik slot untuk melepasnya; keping salah belum diucapkan ke Bima.";
            ShowCardArtwork(wordCardArtwork, wordCardImage, DrawPuzzle);
        }
        void DrawPuzzle()
        {
            var panel = Modal("KARTU KATA • " + puzzle.Id + " — " + puzzle.Title);
            Text(panel, "Context", puzzle.Context, new Vector2(0, 250), new Vector2(1100, 100), 26);
            for (int i = 0; i < 3; i++)
            {
                int slot = i; string label = i < selected.Count ? puzzle.TextFor(selected[i]) : "Slot " + (i + 1);
                var slotButton = StoryOverlay.Button(panel, label, new Vector2((i - 1) * 360, 130), new Vector2(345, 95), () =>
                {
                    selected.RemoveAt(slot);
                    RedrawPuzzle();
                });
                slotButton.interactable = slot < selected.Count;
            }
            // Stable, shuffled presentation; correct phrases are never preselected.
            var phrases = dayTwoProgress == null
                ? puzzle.Phrases.OrderBy(p => p.id).ToArray()
                : puzzle.Phrases.OrderBy(p => Array.IndexOf(dayTwoProgress.wordOrder, p.id)).ToArray();
            Button firstFreeTile = null;
            for (int i = 0; i < phrases.Length; i++)
            {
                var phrase = phrases[i];
                var b = StoryOverlay.Button(panel, phrase.text, new Vector2((i % 3 - 1) * 360, -i / 3 * 95), new Vector2(345, 80), () => { if (selected.Count < 3 && !selected.Contains(phrase.id)) { selected.Add(phrase.id); RedrawPuzzle(); } });
                b.interactable = selected.Count < 3 && !selected.Contains(phrase.id);
                if (b.interactable && firstFreeTile == null) firstFreeTile = b;
            }
            int failureIndex = puzzle.Id == "K-01" ? 0 : puzzle.Id == "K-02" ? 1 : 2;
            string feedback = puzzleFeedback + (failures[failureIndex] >= 2 ? "\nPetunjuk: " + puzzle.Hint : "");
            Text(panel, "Feedback", feedback, new Vector2(0, -188), new Vector2(1100, 95), 24);
            puzzleHelp = Text(panel, "Control hint", FormatControl(submitPrompt, "Pilih tombol yang disorot"),
                new Vector2(0, -272), new Vector2(1100, 40), 22);
            StoryOverlay.Button(panel, "Susun ulang", new Vector2(-400, -345), new Vector2(260, 65), () => { selected.Clear(); RedrawPuzzle(); });
            if (dayTwoProgress != null) StoryOverlay.Button(panel, "Kembali", new Vector2(0, -345), new Vector2(260, 65), closePuzzle);
            else if (failures[failureIndex] >= 4) StoryOverlay.Button(panel, "Bantu susun", new Vector2(0, -345), new Vector2(260, 65), () => { selected.Clear(); selected.AddRange(puzzle.Answer); DrawPuzzle(); });
            var check = StoryOverlay.Button(panel, dayTwoProgress != null ? "Periksa" : "Ucapkan", new Vector2(400, -345), new Vector2(260, 65), () =>
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
            if (dayTwoProgress != null)
            {
                check.interactable = selected.Count == 3;
                cancelModal = closePuzzle;
            }
            Select(firstFreeTile != null ? firstFreeTile : check);
        }
        public void ShowJournal(string body, Action close)
        {
            var panel = Modal("JURNAL • TUJUAN, PETUNJUK & PEMBELAJARAN");
            var viewport = StoryOverlay.Rect(panel, "Viewport", new Vector2(0, 5), new Vector2(1100, 590));
            viewport.gameObject.AddComponent<Image>().color = Color.clear; viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = journalScroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            var text = Text(viewport, "Entries", string.IsNullOrEmpty(body) ? "Catatan akan muncul saat Aji memahami kejadian." : body, Vector2.zero, new Vector2(1060, 590), 26);
            var rect = text.rectTransform; rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(-30, 590);
            text.ForceMeshUpdate(); rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(590, text.preferredHeight + 30));
            scroll.viewport = viewport; scroll.content = rect;
            cancelModal = close;
            Select(StoryOverlay.Button(panel, "Kembali", new Vector2(400, -345), new Vector2(260, 65), close));
        }
        TMP_Text controlHelp, puzzleHelp;
        string controlsText, submitPrompt;
        static string FormatControl(string input, string action) =>
            string.IsNullOrEmpty(input) ? action : $"[{input}] {action}";

        public void SetControlPrompts(string move, string look, string sprint, string jump,
            string interact, string journal, string pause, string submit)
        {
            controlsText = string.Join("\n", new[] { move, look, sprint, jump, interact, journal, pause }
                .Where(label => !string.IsNullOrEmpty(label)));
            submitPrompt = submit;
            if (controlHelp != null) controlHelp.text = controlsText;
            if (puzzleHelp != null) puzzleHelp.text = FormatControl(submitPrompt, "Pilih tombol yang disorot");
        }

        public void ShowPause(Action close, Action saveAction, Action restart)
        {
            var p = Modal("JEDA");
            controlHelp = Text(p, "Controls", controlsText ?? "", new Vector2(0, 40), new Vector2(1100, 500), 29);
            cancelModal = close;
            Select(StoryOverlay.Button(p, "Lanjut", new Vector2(-380, -345), new Vector2(260, 65), close));
            StoryOverlay.Button(p, "Simpan", new Vector2(0, -345), new Vector2(260, 65), saveAction);
            StoryOverlay.Button(p, "Ulang Level 1", new Vector2(380, -345), new Vector2(260, 65), restart);
        }
        public void ConfirmRestart(Action yes, Action no)
        {
            var p = Modal("ULANG LEVEL 1?");
            Text(p, "Warning", "Progres Kartu Aksi dan Kartu Kata Level 1 akan dimulai ulang.", Vector2.zero, new Vector2(1100, 200), 30);
            cancelModal = no;
            Select(StoryOverlay.Button(p, "Batal", new Vector2(-220, -345), new Vector2(260, 65), no));
            StoryOverlay.Button(p, "Ulang", new Vector2(220, -345), new Vector2(260, 65), yes);
        }
    }
}

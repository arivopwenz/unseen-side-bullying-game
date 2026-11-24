using System.Collections.Generic;
using BullyingGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BullyingGame.Prologue
{
    public sealed class PrologueHud : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Vector2 mapMin = new Vector2(-93, -55);
        [SerializeField] private Vector2 mapMax = new Vector2(48, 33);

        private static readonly Color Gold = new Color32(255, 212, 91, 255);
        private static readonly Color Muted = new Color32(170, 185, 183, 255);
        private static readonly Color PanelColor = new Color32(22, 30, 32, 246);
        private readonly List<Object> runtimeAssets = new List<Object>();
        private PrologueFlow flow;
        private PrologueFlow.Territory[] territories;
        private Transform player, target;
        private Vector3? routeHint;
        private Text lesson, objective, prompt, badge, distance, mapHint, mapBadge;
        private Image miniPlayer, miniTarget, direction;
        private Image mapPlayer;
        private readonly List<(RectTransform rect, Vector2 world)> miniBuildings = new List<(RectTransform, Vector2)>();
        private GameObject map;
        private Button[] buttons;
        private Button closeButton;
        private string walk, jump, sprint, interact;
        private Text[] buttonLabels;
        private Image[] pins;
        private float badgeUntil;
        private Font font;

        internal void Initialize(PrologueFlow owner, PrologueFlow.Territory[] rooms, Transform actor)
        {
            flow = owner;
            territories = rooms;
            player = actor;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Sprite circle = MakeSprite(Minimap.MakeCircleTexture(96));
            Sprite triangle = MakeSprite(Minimap.MakeTriangleTexture(64));
            RectTransform root = AddRect(canvas.transform, "Prologue HUD", new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.sizeDelta = Vector2.zero;
            Vector2 topLeft = new Vector2(0, 1);
            Vector2 topRight = Vector2.one;
            Vector2 top = new Vector2(.5f, 1);

            bool onRight = gameObject.scene.name == "Tutorial";
            BuildMinimap(root, circle, triangle, onRight);
            mapHint = AddText(root, "", onRight ? topRight : topLeft, new Vector2(onRight ? -102 : 119, -197), new Vector2(230, 30), 20);
            lesson = AddText(root, "", onRight ? topLeft : topRight, new Vector2(onRight ? 198 : -198, -115), new Vector2(340, 180), 27, TextAnchor.MiddleLeft);
            objective = AddText(root, "", onRight ? topLeft : topRight, new Vector2(onRight ? 258 : -258, -48), new Vector2(466, 53), 24, onRight ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
            prompt = AddText(root, "", new Vector2(.5f, 0), new Vector2(0, 88), new Vector2(630, 56), 27);
            badge = AddText(root, "", top, new Vector2(0, -37), new Vector2(520, 62), 25);
            badge.color = Gold;
            direction = AddImage(root, "Panah GPS", top, new Vector2(0, -100), new Vector2(40, 40), Gold, triangle);
            distance = AddText(root, "", top, new Vector2(0, -140), new Vector2(115, 35), 24);
            BuildMap(root, circle, triangle);
        }

        internal void InitializeMinimap(RectTransform parent, Transform actor)
        {
            player = actor;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildMinimap(parent, MakeSprite(Minimap.MakeCircleTexture(96)),
                MakeSprite(Minimap.MakeTriangleTexture(64)), true);
        }

        internal void SetMinimapTarget(Transform destination)
        {
            target = destination;
            miniTarget.gameObject.SetActive(target != null);
        }

        private void BuildMinimap(RectTransform root, Sprite circle, Sprite triangle, bool onRight)
        {
            var mini = AddImage(root, "Minimap", onRight ? Vector2.one : new Vector2(0, 1), new Vector2(onRight ? -102 : 102, -102), new Vector2(156, 156), PanelColor, circle);
            var miniArea = AddImage(mini.transform, "Area", Vector2.one * .5f, Vector2.zero, new Vector2(146, 146),
                new Color32(54, 75, 67, 255), circle);
            miniArea.gameObject.AddComponent<Mask>();
            BuildCampus(miniArea.transform, true);
            AddText(mini.transform, "N", new Vector2(.5f, 1), new Vector2(0, -13), new Vector2(22, 22), 15);
            miniTarget = AddImage(mini.transform, "Objective dot", Vector2.one * .5f, Vector2.zero, new Vector2(15, 15), Gold, circle);
            miniPlayer = AddImage(mini.transform, "Player", Vector2.one * .5f, Vector2.zero, new Vector2(19, 19), Color.white, triangle);
        }

        internal void SetTutorial(bool moved, bool jumped, bool sprinted, bool hide)
        {
            lesson.gameObject.SetActive(!hide);
            lesson.text = "Tutorial\n" + Control(walk, moved) + "\n" + Control(jump, jumped) +
                "\n" + Control(sprint, sprinted) + "\n" + interact;
        }

        internal void SetBindings(string moveKey, string jumpKey, string sprintKey, string interaction, string mapKey)
        {
            walk = $"[{moveKey}]  Jalan";
            jump = $"[{jumpKey}]  Lompat";
            sprint = $"[{sprintKey}]  Lari";
            interact = interaction + "Interaksi";
            mapHint.text = $"[{mapKey}]  GPS";
        }

        private static string Control(string label, bool done) => done ? "<color=#aedab6>" + label + "</color>" : label;

        internal void SetMapAvailable(bool available) => mapHint.gameObject.SetActive(available);
        internal void SetRouteHint(Vector3 waypoint) => routeHint = waypoint;

        internal void SetObjective(string text, Transform destination, bool gps)
        {
            objective.text = text;
            objective.rectTransform.anchoredPosition = new Vector2(gameObject.scene.name == "Tutorial" ? 258 : -258, text == "Bicara dengan Bima" ? -224 : -48);
            target = destination;
            routeHint = null;
            miniTarget.gameObject.SetActive(destination != null);
            direction.gameObject.SetActive(gps);
            distance.gameObject.SetActive(gps);
            if (destination == null) lesson.gameObject.SetActive(false);
        }

        internal void SetPrompt(string text) => prompt.text = text;

        internal void ShowBadge(string room, int count, int total)
        {
            badge.text = $"Badge Penjelajah Sekolah  {count}/{total}\n{room}";
            badgeUntil = Time.unscaledTime + 4.5f;
        }

        internal void ShowMap(bool visible)
        {
            map.SetActive(visible);
            if (!visible) return;
            int count = 0;
            Button first = null;
            for (int i = 0; i < territories.Length; i++)
            {
                bool visited = flow.IsVisited(i);
                if (visited) count++;
                bool selectable = flow.CanSelect(i);
                buttonLabels[i].text = territories[i].name + (visited ? "  •  Dikunjungi" :
                    selectable ? "  •  Pilih tujuan" : "  •  Terkunci");
                buttonLabels[i].color = selectable ? Color.white : Muted;
                buttons[i].interactable = selectable && !visited;
                if (first == null && buttons[i].interactable) first = buttons[i];
                pins[i].gameObject.SetActive(selectable);
                pins[i].color = visited ? new Color32(158, 226, 185, 255) : Gold;
            }
            mapBadge.text = $"Penjelajah Sekolah  {count}/{territories.Length}";
            (first != null ? first : closeButton).Select();
        }

        private void LateUpdate()
        {
            if (player == null) return;
            if (badge != null) badge.gameObject.SetActive(Time.unscaledTime < badgeUntil);
            float yaw = player.eulerAngles.y;
            miniPlayer.rectTransform.localRotation = Quaternion.Euler(0, 0, -yaw);
            Vector3 position = player.position;
            Vector2 playerXZ = new Vector2(position.x, position.z);
            foreach (var building in miniBuildings)
                building.rect.anchoredPosition = (building.world - playerXZ) * 2.9f;
            if (target != null)
            {
                Vector3 delta = target.position - player.position;
                Vector2 planar = new Vector2(delta.x, delta.z);
                miniTarget.rectTransform.anchoredPosition = Vector2.ClampMagnitude(planar * 2.9f, 60f);
                if (direction == null) return;
                var camera = UnityEngine.Camera.main;
                Vector3 forward = camera != null ? camera.transform.forward : player.forward;
                Vector3 heading = Vector3.ProjectOnPlane((routeHint ?? target.position) - position, Vector3.up);
                float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(forward, Vector3.up), heading, Vector3.up);
                direction.rectTransform.localRotation = Quaternion.Euler(0, 0, -angle);
                distance.text = Mathf.RoundToInt(planar.magnitude) + " m";
            }
            if (map != null && map.activeSelf)
            {
                mapPlayer.rectTransform.anchoredPosition = MapPosition(player.position);
                mapPlayer.rectTransform.localRotation = miniPlayer.rectTransform.localRotation;
            }
        }

        private void BuildMap(Transform root, Sprite circle, Sprite triangle)
        {
            Vector2 center = Vector2.one * .5f;
            var overlay = AddImage(root, "Peta / GPS", center, Vector2.zero, Vector2.zero, new Color32(5, 10, 12, 198));
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.sizeDelta = Vector2.zero;
            overlay.raycastTarget = true;
            map = overlay.gameObject;
            var panel = AddImage(map.transform, "Panel", center, Vector2.zero, new Vector2(960, 515), PanelColor);
            AddText(panel.transform, "Peta Sekolah", new Vector2(0, 1), new Vector2(150, -50), new Vector2(244, 45), 34, TextAnchor.MiddleLeft);
            closeButton = AddButton(panel.transform, "Tutup", new Vector2(372, 201), new Vector2(154, 45), () => flow.CloseMap());
            var layout = AddImage(panel.transform, "Lokasi", center, new Vector2(-212, -32), new Vector2(440, 350),
                new Color32(41, 64, 59, 255));
            BuildCampus(layout.transform, false);
            AddText(layout.transform, "N", new Vector2(.5f, 1), new Vector2(0, -17), new Vector2(30, 22), 18);
            buttons = new Button[territories.Length];
            buttonLabels = new Text[territories.Length];
            pins = new Image[territories.Length];
            for (int i = 0; i < territories.Length; i++)
            {
                int index = i;
                buttons[i] = AddButton(panel.transform, territories[i].name,
                    new Vector2(225, 117 - i * Mathf.Min(54, 315f / Mathf.Max(1, territories.Length - 1))),
                    new Vector2(395, Mathf.Min(46, 278f / Mathf.Max(1, territories.Length - 1))), () => flow.SelectTerritory(index));
                buttonLabels[i] = buttons[i].GetComponentInChildren<Text>();
                pins[i] = AddImage(layout.transform, territories[i].name, center, MapPosition(territories[i].arrival.position),
                    new Vector2(17, 17), Gold, circle);
                AddText(pins[i].transform, territories[i].name, center, new Vector2(0, -19), new Vector2(135, 24), 16);
            }
            mapBadge = AddText(panel.transform, "", center, new Vector2(225, -211), new Vector2(390, 33), 21);
            mapBadge.color = Gold;
            mapPlayer = AddImage(layout.transform, "Aji", center, Vector2.zero, new Vector2(24, 24), Color.white, triangle);
            map.SetActive(false);
        }

        private void BuildCampus(Transform layout, bool minimap)
        {
            Color paving = new Color32(96, 111, 101, 255);
            Color buildings = new Color32(118, 130, 121, 255);
            Color field = new Color32(60, 103, 77, 255);
            AddFootprint("Koridor", new Vector2(-24, 6), new Vector2(101, 12), paving);
            AddFootprint("Lapangan", new Vector2(-27, -10), new Vector2(56, 24), field);
            AddFootprint("Jalan masuk", new Vector2(-17, -36), new Vector2(12, 28), paving);
            AddFootprint("Ruang kelas", new Vector2(-10, 21), new Vector2(80, 15), buildings);
            AddFootprint("Bangunan timur", new Vector2(42, -14), new Vector2(12, 50), buildings);
            AddFootprint("Bangunan barat", new Vector2(-75, -9), new Vector2(19, 23), buildings);

            void AddFootprint(string label, Vector2 center, Vector2 size, Color color)
            {
                Vector2 position;
                if (minimap) position = Vector2.zero;
                else position = MapPosition(new Vector3(center.x, 0, center.y));
                Vector2 scale = minimap ? Vector2.one * 2.9f :
                    new Vector2(370f / (mapMax.x - mapMin.x), 280f / (mapMax.y - mapMin.y));
                var image = AddImage(layout, label, Vector2.one * .5f, position, Vector2.Scale(size, scale), color);
                if (minimap) miniBuildings.Add((image.rectTransform, center));
            }
        }

        private Vector2 MapPosition(Vector3 point) => new Vector2(
            Mathf.Lerp(-185, 185, Mathf.InverseLerp(mapMin.x, mapMax.x, point.x)),
            Mathf.Lerp(-140, 140, Mathf.InverseLerp(mapMin.y, mapMax.y, point.z)));

        private Sprite MakeSprite(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
            runtimeAssets.Add(sprite);
            runtimeAssets.Add(texture);
            return sprite;
        }

        private void OnDestroy()
        {
            foreach (var asset in runtimeAssets) Destroy(asset);
        }

        private static RectTransform AddRect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image AddImage(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color, Sprite sprite = null)
        {
            var image = AddRect(parent, name, anchor, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private Text AddText(Transform parent, string value, Vector2 anchor, Vector2 position, Vector2 size, int fontSize,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var text = AddRect(parent, string.IsNullOrEmpty(value) ? "Label" : value, anchor, position, size).gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.text = value;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        private Button AddButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction click)
        {
            var image = AddImage(parent, label, Vector2.one * .5f, position, size, new Color32(57, 78, 74, 255));
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);
            AddText(button.transform, label, Vector2.one * .5f, Vector2.zero, size, 22);
            return button;
        }
    }
}

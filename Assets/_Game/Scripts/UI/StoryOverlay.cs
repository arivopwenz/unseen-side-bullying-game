using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

namespace BullyingGame.UI
{
    public class StoryOverlay : MonoBehaviour
    {
        public static StoryOverlay Current { get; private set; }
        public static int DismissedFrame { get; private set; } = -1;
        public RectTransform Content { get; private set; }
        public TMP_Text Body { get; private set; }
        private Action dismissed;
        private InputAction cancel;
        public static StoryOverlay Show(string title, string body, string[] labels, Action<int> clicked, Action onDismiss = null)
        {
            if (Current != null) Current.Dismiss();
            var root = new GameObject("StoryOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(StoryOverlay));
            var view = root.GetComponent<StoryOverlay>(); Current = view; view.dismissed = onDismiss;
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f;
            var dim = Rect(root.transform,"Backdrop",Vector2.zero,new Vector2(4000,3000)); dim.gameObject.AddComponent<Image>().color = new Color(.015f,.025f,.04f,.8f);
            var panel = Rect(root.transform,"Panel",Vector2.zero,new Vector2(1180,850)); panel.gameObject.AddComponent<Image>().color = new Color(.035f,.07f,.09f,1);
            Text(panel,"Title",title,new Vector2(0,358),new Vector2(1080,65),34);
            var viewport = Rect(panel,"Viewport",new Vector2(0,10),new Vector2(1080,610)); viewport.gameObject.AddComponent<Image>().color = Color.clear; viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            view.Content = Rect(viewport,"Content",Vector2.zero,new Vector2(1080,610)); view.Content.anchorMin = new Vector2(0,1); view.Content.anchorMax = new Vector2(1,1); view.Content.pivot = new Vector2(.5f,1); view.Content.anchoredPosition = Vector2.zero; view.Content.sizeDelta = new Vector2(0,610);
            view.Body = Text(view.Content,"Body",body,Vector2.zero,new Vector2(1060,610),26 * PlayerPrefs.GetFloat("Story.TextScale",1));
            var br = view.Body.rectTransform; br.anchorMin = new Vector2(0,1); br.anchorMax = new Vector2(1,1); br.pivot = new Vector2(.5f,1); br.anchoredPosition = Vector2.zero; br.sizeDelta = new Vector2(-20,610);
            view.Body.ForceMeshUpdate(); float height = Mathf.Max(610,view.Body.preferredHeight+20); br.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height); view.Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
            scroll.viewport = viewport; scroll.content = view.Content;
            for (int i=0; i<labels.Length; i++)
            {
                int index = i; float width = Mathf.Min(250,1040f / labels.Length);
                var button = Button(panel, labels[i], new Vector2((i-(labels.Length-1)*.5f)*(width+10),-355), new Vector2(width,66),()=>clicked?.Invoke(index));
                if(i==0) EventSystem.current?.SetSelectedGameObject(button.gameObject);
            }
            view.cancel = new InputAction("ClosePanel",InputActionType.Button,"<Keyboard>/escape"); view.cancel.performed += _ => view.Dismiss(); view.cancel.Enable();
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            return view;
        }
        public void Dismiss()
        {
            if(Current!=this) return; Current=null; DismissedFrame=Time.frameCount;
            cancel?.Disable(); var callback=dismissed; dismissed=null; Destroy(gameObject); callback?.Invoke();
        }
        public static RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size)
        {
            var g = new GameObject(name,typeof(RectTransform)); g.transform.SetParent(parent,false);
            var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        public static TMP_Text Text(Transform parent,string name,string value,Vector2 pos,Vector2 size,float font=26)
        {
            var r=Rect(parent,name,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=TMP_Settings.defaultFontAsset;t.text=value;t.fontSize=font;t.color=new Color(.93f,.97f,.98f);t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;
        }
        public static Button Button(Transform parent,string label,Vector2 pos,Vector2 size,Action action)
        {
            var r=Rect(parent,label,pos,size);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.10f,.25f,.28f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;
            var colors=b.colors;colors.highlightedColor=new Color(.55f,.9f,.8f);colors.selectedColor=new Color(.55f,.9f,.8f);b.colors=colors;
            var t=Text(r,"Label",label,Vector2.zero,size-new Vector2(16,8),22);t.alignment=TextAlignmentOptions.Center;b.onClick.AddListener(()=>action?.Invoke());return b;
        }
        private void OnDestroy() { cancel?.Dispose();if(Current==this)Current=null; }
    }
}

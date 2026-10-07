using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BullyingGame.Core;
using BullyingGame.UI;

public static class MenuAndInput
{
    static T C<T>(GameObject go) where T:Component {var c=go.GetComponent<T>();if(c==null)c=go.AddComponent<T>();return c;}
    static void Ref(UnityEngine.Object obj,string name,UnityEngine.Object value){var so=new SerializedObject(obj);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj);}
    static GameObject Rect(Transform parent,string name,Vector2 anchor,Vector2 pivot,Vector2 position,Vector2 size)
    {var t=parent.Find(name);var go=t!=null?t.gameObject:new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=position;r.sizeDelta=size;return go;}
    static TextMeshProUGUI Label(Transform parent,string name,string text,Vector2 position,Vector2 size,float font)
    {var go=Rect(parent,name,new Vector2(.5f,.5f),new Vector2(.5f,.5f),position,size);var label=C<TextMeshProUGUI>(go);label.font=TMP_Settings.defaultFontAsset;label.text=text;label.fontSize=font;label.color=new Color(.91f,.96f,.98f);label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;return label;}
    static Button Button(Transform parent,string name,string text,Vector2 position,Vector2 size)
    {var go=Rect(parent,name,new Vector2(.5f,.5f),new Vector2(.5f,.5f),position,size);var image=C<Image>(go);image.color=new Color(.13f,.32f,.38f,.98f);var b=C<Button>(go);b.targetGraphic=image;b.onClick=new Button.ButtonClickedEvent();Label(go.transform,"Caption",text,Vector2.zero,size-Vector2.one*12,25);EditorUtility.SetDirty(b);return b;}
    static void Binding(InputActionAsset asset,string action,string path,string processors=null)
    {var a=asset.FindAction(action,true);if(!a.bindings.Any(b=>b.path==path))a.AddBinding(path,processors:processors);}
    static void RemoveMissing(UnityEngine.SceneManagement.Scene scene)
    {foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);}
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        var level=EditorSceneManager.GetActiveScene();if(level.name!="Level01")throw new Exception("Open Level01 first");
        var story=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>();var safe=story.transform.Find("SafeArea");
        var mobileRoot=Rect(safe,"TouchControls",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(1920,1080));
        var mobile=C<MobileGameplayUI>(mobileRoot);var movement=Rect(mobileRoot.transform,"MovementControls",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(1920,1080));
        foreach(var entry in new[]{new{ name="MoveStick",pos=new Vector2(-730,-280),path="<Gamepad>/leftStick",caption="Gerak"},new{name="LookStick",pos=new Vector2(580,-280),path="<Gamepad>/rightStick",caption="Kamera"}})
        {var bg=Rect(movement.transform,entry.name+"Base",new Vector2(.5f,.5f),new Vector2(.5f,.5f),entry.pos,new Vector2(160,160));C<Image>(bg).color=new Color(.1f,.21f,.27f,.55f);var stickHandle=Rect(bg.transform,"Handle",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(78,78));C<Image>(stickHandle).color=new Color(.47f,.79f,.78f,.85f);var stick=C<OnScreenStick>(stickHandle);stick.controlPath=entry.path;stick.movementRange=62;Label(bg.transform,"Caption",entry.caption,new Vector2(0,-115),new Vector2(220,35),25);}
        var interact=Button(movement.transform,"Interact","Bicara / Ambil",new Vector2(760,-390),new Vector2(280,80));C<OnScreenButton>(interact.gameObject).controlPath="<Gamepad>/buttonWest";
        var sprint=Button(movement.transform,"Sprint","Lari",new Vector2(-520,-420),new Vector2(170,65));C<OnScreenButton>(sprint.gameObject).controlPath="<Gamepad>/leftShoulder";
        var tap=Button(mobileRoot.transform,"QTETap","TEKAN",new Vector2(780,-270),new Vector2(230,140));C<OnScreenButton>(tap.gameObject).controlPath="<Gamepad>/buttonSouth";
        var pause=Button(mobileRoot.transform,"PauseTouch","Jeda",new Vector2(800,360),new Vector2(180,65));C<OnScreenButton>(pause.gameObject).controlPath="<Gamepad>/start";
        Ref(mobile,"movementGroup",movement);Ref(mobile,"qteButton",tap.gameObject);Ref(mobile,"pauseButton",pause.gameObject);
        movement.SetActive(false);tap.gameObject.SetActive(false);pause.gameObject.SetActive(false);
        QualitySettings.vSyncCount=1;QualitySettings.shadowDistance=35;QualitySettings.lodBias=1;
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.runInBackground=true;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
        RemoveMissing(level);EditorSceneManager.MarkSceneDirty(level);EditorSceneManager.SaveScene(level);
        var boot=EditorSceneManager.OpenScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity",OpenSceneMode.Additive);
        RemoveMissing(boot);var bootGo=boot.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="Bootstrap").gameObject;C<BootstrapLauncher>(bootGo);
        foreach(var states in boot.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<GameStateManager>(true)))states.transform.SetParent(null,true);
        EditorSceneManager.MarkSceneDirty(boot);EditorSceneManager.SaveScene(boot);EditorSceneManager.CloseScene(boot,true);
        var menu=EditorSceneManager.OpenScene("Assets/_Game/Scenes/01_MainMenu/01_MainMenu.unity",OpenSceneMode.Additive);RemoveMissing(menu);
        var canvas=menu.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Canvas>(true)).First();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=C<CanvasScaler>(canvas.gameObject);scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        foreach(var child in Enumerable.Range(0,canvas.transform.childCount).Select(i=>canvas.transform.GetChild(i)).ToArray())child.gameObject.SetActive(false);
        var background=Rect(canvas.transform,"StoryMenuBackground",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(1920,1080));background.SetActive(true);C<Image>(background).color=new Color(.045f,.1f,.14f);
        Label(background.transform,"Title","THE UNSEEN SIDE",new Vector2(0,355),new Vector2(1500,120),76);
        Label(background.transform,"Subtitle","Lima bab. Tiga perspektif. Satu lingkungan yang perlu berubah.",new Vector2(0,250),new Vector2(1600,75),29);
        Label(background.transform,"Description","Jalani kisah Arga, Nara, dan Raka di ruang terbuka.\nDengarkan, cari dukungan, hormati batas, dan lihat dampak setiap langkah.",new Vector2(0,130),new Vector2(1350,100),26);
        var flow=C<MainMenuFlow>(canvas.gameObject);
        Ref(flow,"newGameButton",Button(background.transform,"NewGame","Mulai baru",new Vector2(0,15),new Vector2(630,70)));
        Ref(flow,"continueButton",Button(background.transform,"Continue","Lanjutkan checkpoint",new Vector2(0,-80),new Vector2(630,70)));
        Ref(flow,"settingsButton",Button(background.transform,"Settings","Pengaturan suara",new Vector2(-165,-175),new Vector2(300,65)));
        Ref(flow,"quitButton",Button(background.transform,"Quit","Keluar",new Vector2(165,-175),new Vector2(300,65)));
        var status=Label(background.transform,"Status","",new Vector2(0,-340),new Vector2(1600,95),22);Ref(flow,"statusText",status);
        Label(background.transform,"NewGameNote","Mulai baru mengganti checkpoint cerita sebelumnya.",new Vector2(0,-420),new Vector2(1500,40),19);
        var settings=Rect(background.transform,"VolumeSettings",new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(610,-90),new Vector2(480,250));C<Image>(settings).color=new Color(.09f,.23f,.28f);Label(settings.transform,"VolumeCaption","Volume utama",new Vector2(0,75),new Vector2(420,45),28);
        var sliderGo=Rect(settings.transform,"Volume",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(390,32));C<Image>(sliderGo).color=new Color(.22f,.35f,.4f);var fillGo=Rect(sliderGo.transform,"Fill",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(390,32));C<Image>(fillGo).color=new Color(.4f,.78f,.72f);var handle=Rect(sliderGo.transform,"Handle",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(32,48));C<Image>(handle).color=Color.white;var slider=C<Slider>(sliderGo);slider.fillRect=fillGo.GetComponent<RectTransform>();slider.handleRect=handle.GetComponent<RectTransform>();slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=1;Ref(flow,"settingsPanel",settings);Ref(flow,"volumeSlider",slider);settings.SetActive(false);
        foreach(var states in menu.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<GameStateManager>(true)))states.transform.SetParent(null,true);
        EditorSceneManager.MarkSceneDirty(menu);EditorSceneManager.SaveScene(menu);EditorSceneManager.CloseScene(menu,true);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity",true),new EditorBuildSettingsScene("Assets/_Game/Scenes/01_MainMenu/01_MainMenu.unity",true),new EditorBuildSettingsScene(level.path,true)};
        string inputPath="Assets/_Game/Settings/GameInput.inputactions";var input=InputActionAsset.FromJson(File.ReadAllText(inputPath));
        Binding(input,"Movement/Move","<Gamepad>/leftStick");Binding(input,"Movement/Look","<Gamepad>/rightStick","ScaleVector2(x=100,y=100)");Binding(input,"Movement/Interact","<Gamepad>/buttonWest");Binding(input,"Movement/Sprint","<Gamepad>/leftShoulder");Binding(input,"Movement/Pause","<Gamepad>/start");Binding(input,"Dialogue/Advance","<Gamepad>/buttonSouth");Binding(input,"QTE/Tap","<Gamepad>/buttonSouth");
        File.WriteAllText(inputPath,input.ToJson());UnityEngine.Object.DestroyImmediate(input);AssetDatabase.ImportAsset(inputPath,ImportAssetOptions.ForceUpdate);AssetDatabase.SaveAssets();
        return "Saved Bootstrap/Main Menu/Continue/volume, build order, desktop settings, mobile action-based sticks/buttons and native Input System bindings. Importer regenerates GameInput.cs.";
    }
}


using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BullyingGame.Core;
using BullyingGame.NPC;
using BullyingGame.Quest;
using BullyingGame.Dialogue;
using BullyingGame.QTE;
using BullyingGame.UI;
using BullyingGame.Cinematics;

public static class FiveChapterScene
{
    const string Data="Assets/_Game/Data/StoryPrototype/";
    const string Art="Assets/_Game/Art/Prototype/";
    static T Component<T>(GameObject go) where T:Component { var c=go.GetComponent<T>();if(c==null)c=go.AddComponent<T>();return c; }
    static GameObject Child(Transform parent,string name)
    { var t=parent.Find(name);if(t!=null)return t.gameObject;var g=new GameObject(name);g.transform.SetParent(parent,false);Undo.RegisterCreatedObjectUndo(g,"Five outdoor chapters");return g; }
    static T Load<T>(string name) where T:UnityEngine.Object { var value=AssetDatabase.LoadAssetAtPath<T>(Data+name+".asset");if(value==null)throw new Exception("Missing "+name);return value; }
    static QuestData Q(string name) => name=="ch01_book" ? AssetDatabase.LoadAssetAtPath<QuestData>("Assets/_Game/Data/Quests/Quest_Level01_LostNotebook.asset") : Load<QuestData>("Quest_"+name);
    static void Ref(UnityEngine.Object obj,string field,UnityEngine.Object value) { var so=new SerializedObject(obj);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj); }
    static void Refs(UnityEngine.Object obj,string field,UnityEngine.Object[] values) { var so=new SerializedObject(obj);var p=so.FindProperty(field);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj); }
    static GameObject Field(UnityEngine.Object obj,string name) { var o=new SerializedObject(obj).FindProperty(name).objectReferenceValue;return o is GameObject g ? g : o is Component c ? c.gameObject : null; }
    static Material Mat(string name,Color color)
    { var m=AssetDatabase.LoadAssetAtPath<Material>(Art+name+".mat");if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,Art+name+".mat");}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.1f);EditorUtility.SetDirty(m);return m; }
    static GameObject Shape(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Material mat)
    { var t=parent.Find(name);var g=t!=null ? t.gameObject : GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;var collider=g.GetComponent<Collider>();if(collider!=null)UnityEngine.Object.DestroyImmediate(collider);return g; }
    static void Conversations(string path,params string[] entries)
    { var npc=GameObject.Find(path).GetComponent<MissionDialogueNPC>();var so=new SerializedObject(npc);var list=so.FindProperty("conversations");list.arraySize=entries.Length;
        for(int i=0;i<entries.Length;i++){var f=entries[i].Split('|');var p=list.GetArrayElementAtIndex(i);p.FindPropertyRelative("quest").objectReferenceValue=Q(f[0]);p.FindPropertyRelative("objectiveId").stringValue=f[1];p.FindPropertyRelative("requiredObjectiveId").stringValue=f[2];p.FindPropertyRelative("dialogue").objectReferenceValue=Load<DialogueData>(f[3]);}so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(npc); }
    static void Rect(GameObject go,Vector2 anchor,Vector2 pivot,Vector2 position,Vector2 size)
    { if(go==null)throw new Exception("Missing UI reference");var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=position;r.sizeDelta=size; }
    static TextMeshProUGUI Label(Transform parent,string name,string text,Vector2 anchor,Vector2 position,Vector2 size,float font)
    { var child=parent.Find(name);var g=child!=null ? child.gameObject : new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);Rect(g,anchor,anchor,position,size);var label=Component<TextMeshProUGUI>(g);label.font=TMP_Settings.defaultFontAsset;label.text=text;label.fontSize=font;label.color=new Color(.93f,.97f,1);label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.Normal;return label; }
    static void Font(GameObject go,float size) { if(go!=null){var t=go.GetComponent<TextMeshProUGUI>();if(t!=null){t.fontSize=size;t.enableAutoSizing=false;t.color=new Color(.92f,.96f,1);}} }
    static void SafeArea(Canvas canvas)
    { if(canvas.renderMode!=RenderMode.ScreenSpaceOverlay)return;var old=canvas.transform.Find("SafeArea");var children=Enumerable.Range(0,canvas.transform.childCount).Select(i=>canvas.transform.GetChild(i)).Where(c=>c!=old).ToArray();var g=old!=null?old.gameObject:new GameObject("SafeArea",typeof(RectTransform));g.transform.SetParent(canvas.transform,false);var rect=g.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;foreach(var child in children)child.SetParent(g.transform,false);Component<SafeAreaPanel>(g); }
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        var sequence=UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();if(sequence==null)throw new Exception("Open Level01 first");
        Refs(sequence,"chapters",Enumerable.Range(1,5).Select(i=>(UnityEngine.Object)Load<NarrativeChapterData>("Chapter0"+i)).ToArray());
        var manager=UnityEngine.Object.FindAnyObjectByType<ChapterManager>();var cs=new SerializedObject(manager);cs.FindProperty("totalChapters").intValue=5;cs.ApplyModifiedPropertiesWithoutUndo();
        Conversations("_Level/StoryCharacters/Dimas","ch01_friend|listen_friend||Dimas_Listen","ch01_extortion|hear_extortion||Dimas_Extortion","ch01_escort|agree_escort||Dimas_Escort","ch01_escort|check_in|reach_safe_zone|Dimas_Safe","ch02_check|check_friend||Witness_Check","ch02_support|share_witness||Witness_Consent","ch03_repair|ask_consent||Bully_Consent","ch04_check|park_check||Park_Check","ch04_boundaries|state_boundary||Park_Boundary","ch05_consent|confirm_boundaries||Together_Boundaries");
        Conversations("_Level/StoryCharacters/Nara","ch01_witness|speak_witness||Nara_Witness","ch04_support|preserve_evidence||Park_Evidence");
        Conversations("_Level/StoryCharacters/Arga","ch02_help|talk_arga||Witness_Arga","ch02_support|clarify_boundaries|share_witness|Witness_Boundaries","ch05_facts|verify_facts||Together_Facts");
        Conversations("_Level/BuNadia","ch01_extortion|seek_support|hear_extortion|Teacher_Extortion","ch01_witness|plan_support|speak_witness|Teacher_Plan","ch02_help|ask_teacher|talk_arga|Witness_Teacher","ch03_accountability|own_actions||Bully_Accountability","ch03_repair|make_plan|ask_consent|Bully_Plan","ch04_support|seek_support|preserve_evidence|Park_Support","ch05_action|follow_up|check_commitment|Together_FollowUp");
        Conversations("_Level/BullyGroup/Bully_Leader","ch01_book|accept_request||Raka_Request","ch01_book|return_notebook|find_notebook|Raka_Return","ch05_action|check_commitment||Together_Commitment");
        var environment=GameObject.Find("_Level/Environtment");var building=Child(environment.transform,"SchoolBackdrop");
        foreach(var t in environment.GetComponentsInChildren<Transform>(true).Where(t=>t.parent==environment.transform && (t.name=="BackBuilding" || t.name.StartsWith("Window_"))).ToArray())t.SetParent(building.transform,true);
        var decorations=Child(environment.transform,"OutdoorChapterSets");var sets=new GameObject[5];
        var leaf=Mat("Leaf",new Color(.22f,.42f,.28f));var wood=Mat("Wood",new Color(.37f,.26f,.17f));var white=Mat("FieldLines",new Color(.86f,.90f,.84f));var soil=Mat("FlowerBed",new Color(.26f,.19f,.15f));
        var treePositions=new[]{new Vector3(-11,0,10),new Vector3(11,0,10),new Vector3(-11,0,-10),new Vector3(11,0,-10)};
        for(int i=0;i<5;i++)
        {
            sets[i]=Child(decorations.transform,new[]{"SchoolCourtyard","SchoolGarden","SportsField","NeighborhoodPark","SchoolCourtyardEvening"}[i]);
            for(int j=0;j<4;j++){Shape(sets[i].transform,"TreeTrunk_"+j,PrimitiveType.Cylinder,treePositions[j]+Vector3.up*1.3f,new Vector3(.45f,1.3f,.45f),wood);Shape(sets[i].transform,"TreeCrown_"+j,PrimitiveType.Sphere,treePositions[j]+Vector3.up*3.2f,new Vector3(2.8f,2.4f,2.8f),leaf);}
            if(i==1 || i==3)for(int j=0;j<4;j++){var bed=new Vector3(-10+j*6,.12f,10);Shape(sets[i].transform,"Bed_"+j,PrimitiveType.Cube,bed,new Vector3(2.5f,.2f,1.5f),soil);for(int k=0;k<5;k++)Shape(sets[i].transform,"Flower_"+j+"_"+k,PrimitiveType.Sphere,bed+new Vector3(-.9f+k*.45f,.3f,0),Vector3.one*.25f,Mat("Flower_"+k,new Color(.75f,.35f+k*.05f,.5f)));}
            if(i==2){Shape(sets[i].transform,"FieldNorth",PrimitiveType.Cube,new Vector3(0,.012f,10),new Vector3(20,.015f,.08f),white);Shape(sets[i].transform,"FieldSouth",PrimitiveType.Cube,new Vector3(0,.012f,-10),new Vector3(20,.015f,.08f),white);Shape(sets[i].transform,"FieldCenter",PrimitiveType.Cube,new Vector3(0,.012f,0),new Vector3(20,.015f,.08f),white);}
            if(i==3){Shape(sets[i].transform,"WalkingPath",PrimitiveType.Cube,new Vector3(0,.008f,0),new Vector3(6,.01f,25),Mat("ParkPath",new Color(.60f,.58f,.49f)));}
            sets[i].SetActive(i==0);
        }
        var outdoor=Component<OutdoorChapterPresentation>(sequence.gameObject);Refs(outdoor,"outdoorSets",sets);Ref(outdoor,"ground",environment.transform.Find("Ground").GetComponent<Renderer>());
        Refs(outdoor,"groundMaterials",new UnityEngine.Object[]{Mat("Courtyard",new Color(.52f,.58f,.58f)),Mat("GardenGround",new Color(.42f,.53f,.38f)),Mat("SportsGround",new Color(.34f,.48f,.38f)),Mat("ParkGround",new Color(.40f,.52f,.34f)),Mat("EveningCourtyard",new Color(.56f,.56f,.48f))});Ref(outdoor,"schoolBuilding",building);Ref(outdoor,"sunlight",GameObject.Find("Sun").GetComponent<Light>());Ref(sequence,"outdoor",outdoor);
        var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();Ref(opening,"dialogueShot",GameObject.Find("Cameras/OpeningVictimCamera").GetComponent<Unity.Cinemachine.CinemachineCamera>());
        var speakerNames=new[]{"Arga","Nara","Raka","Dimas","Bu Nadia","Bima"};var speakerObjects=new[]{GameObject.Find("_Level/StoryCharacters/Arga"),GameObject.Find("_Level/StoryCharacters/Nara"),GameObject.Find("_Level/BullyGroup/Bully_Leader"),GameObject.Find("_Level/StoryCharacters/Dimas"),GameObject.Find("_Level/BuNadia"),GameObject.Find("_Level/BullyGroup/Bully_Friends")};
        var so=new SerializedObject(opening);var speakers=so.FindProperty("speakers");speakers.arraySize=speakerNames.Length;for(int i=0;i<speakerNames.Length;i++){var s=speakers.GetArrayElementAtIndex(i);s.FindPropertyRelative("speaker").stringValue=speakerNames[i];s.FindPropertyRelative("actor").objectReferenceValue=speakerObjects[i].transform;}so.ApplyModifiedPropertiesWithoutUndo();
        foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        { if(canvas.renderMode!=RenderMode.ScreenSpaceOverlay)continue;var scaler=Component<CanvasScaler>(canvas.gameObject);scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;SafeArea(canvas); }
        var dialogue=UnityEngine.Object.FindAnyObjectByType<DialogueUI>();dialogue.GetComponentInParent<Canvas>().sortingOrder=50;
        var panel=Field(dialogue,"dialoguePanel");Rect(panel,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,35),new Vector2(1560,235));panel.GetComponent<Image>().color=new Color(.04f,.10f,.13f,.96f);
        Rect(Field(dialogue,"speakerNameText"),new Vector2(0,1),new Vector2(0,1),new Vector2(32,-20),new Vector2(1400,36));Font(Field(dialogue,"speakerNameText"),27);
        Rect(Field(dialogue,"dialogueText"),new Vector2(0,1),new Vector2(0,1),new Vector2(32,-69),new Vector2(1490,100));Font(Field(dialogue,"dialogueText"),26);
        var next=Field(dialogue,"continueButton");Rect(next,new Vector2(1,0),new Vector2(1,0),new Vector2(-28,15),new Vector2(230,42));foreach(var label in next.GetComponentsInChildren<TextMeshProUGUI>()){label.text="Lanjut  [E / Enter]";label.fontSize=20;}
        var hud=UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>();var hudPanel=Field(hud,"hudContainer");if(hudPanel==null)hudPanel=hud.GetComponentsInChildren<Transform>(true).First(t=>t.name=="QuestPanel").gameObject;
        Rect(hudPanel,new Vector2(0,1),new Vector2(0,1),new Vector2(26,-24),new Vector2(600,225));hudPanel.GetComponent<Image>().color=new Color(.04f,.1f,.13f,.92f);
        var title=Field(hud,"questTitleText");var objectives=Field(hud,"questObjectiveText");if(title==null)title=hudPanel.transform.Find("QuestTitle").gameObject;if(objectives==null)objectives=hudPanel.transform.Find("QuestObjective").gameObject;
        Ref(hud,"hudContainer",hudPanel);Ref(hud,"questTitleText",title.GetComponent<TextMeshProUGUI>());Ref(hud,"questObjectiveText",objectives.GetComponent<TextMeshProUGUI>());
        Rect(title,new Vector2(0,1),new Vector2(0,1),new Vector2(22,-18),new Vector2(556,55));Font(title,25);Rect(objectives,new Vector2(0,1),new Vector2(0,1),new Vector2(22,-84),new Vector2(556,130));Font(objectives,20);
        var qte=UnityEngine.Object.FindAnyObjectByType<QTEUI>();qte.GetComponentInParent<Canvas>().sortingOrder=60;var qtePanel=Field(qte,"qtePanel");Rect(qtePanel,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,70),new Vector2(690,300));qtePanel.GetComponent<Image>().color=new Color(.04f,.1f,.13f,.96f);
        var fields=new[]{"instructionText","timerText","progressText","resultText"};var positions=new[]{new Vector2(0,-25),new Vector2(280,-93),new Vector2(280,-160),new Vector2(0,-225)};var sizes=new[]{new Vector2(640,54),new Vector2(110,35),new Vector2(110,35),new Vector2(640,48)};
        for(int i=0;i<fields.Length;i++){var g=Field(qte,fields[i]);Rect(g,new Vector2(.5f,1),new Vector2(.5f,1),positions[i],sizes[i]);Font(g,i==0?28:23);g.GetComponent<TextMeshProUGUI>().alignment=TextAlignmentOptions.Center;}
        for(int i=0;i<2;i++){var bar=Field(qte,i==0?"timerBar":"progressBar");Rect(bar,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-45,i==0?-105:-172),new Vector2(490,25));var image=bar.GetComponent<Image>();image.type=Image.Type.Filled;image.fillMethod=Image.FillMethod.Horizontal;image.color=i==0?new Color(.98f,.7f,.27f):new Color(.38f,.83f,.70f);}
        var storyCanvas=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>().GetComponent<Canvas>();var parent=storyCanvas.transform.Find("SafeArea");
        var hint=Label(parent,"NavigationHint","",new Vector2(.5f,0),new Vector2(0,28),new Vector2(1000,42),24);hint.alignment=TextAlignmentOptions.Center;
        var guide=Component<MissionNavigationHint>(storyCanvas.gameObject);Ref(guide,"sequence",sequence);Ref(guide,"player",GameObject.Find("Player").transform);Ref(guide,"hintText",hint);Refs(guide,"characters",UnityEngine.Object.FindObjectsByType<MissionDialogueNPC>(FindObjectsInactive.Include));Ref(guide,"notebook",UnityEngine.Object.FindAnyObjectByType<QuestItem>());Ref(guide,"companion",UnityEngine.Object.FindAnyObjectByType<EscortCompanion>());
        var test=GameObject.Find("_Level/Test_Interactable");if(test!=null)test.SetActive(false);
        var surface=environment.GetComponent<Unity.AI.Navigation.NavMeshSurface>();surface.useGeometry=UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;surface.BuildNavMesh();if(surface.navMeshData!=null && !EditorUtility.IsPersistent(surface.navMeshData))AssetDatabase.CreateAsset(surface.navMeshData,AssetDatabase.GenerateUniqueAssetPath(Art+"NavMesh_School.asset"));
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(sequence.gameObject.scene);if(!EditorSceneManager.SaveScene(sequence.gameObject.scene))throw new Exception("Scene save failed");
        return "Saved 5 outdoor environment sets, 22 mission routes, adaptive opening speaker shots, readable dialogue/QTE/quest HUD, safe area and navigation hint.";
    }
}

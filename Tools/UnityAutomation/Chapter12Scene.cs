using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;
using BullyingGame.Core;
using BullyingGame.NPC;
using BullyingGame.Quest;
using BullyingGame.Dialogue;
using BullyingGame.Events;
using BullyingGame.UI;
using BullyingGame.Cinematics;

public static class Chapter12Scene
{
    const string Data="Assets/_Game/Data/StoryPrototype/";
    const string Art="Assets/_Game/Art/Prototype/";
    static T Load<T>(string name) where T:UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(Data+name+".asset");
    static QuestData Q(string name) => name=="book" ? AssetDatabase.LoadAssetAtPath<QuestData>("Assets/_Game/Data/Quests/Quest_Level01_LostNotebook.asset") : Load<QuestData>("Quest_"+name);
    static T C<T>(GameObject go) where T:Component {var c=go.GetComponent<T>();if(c==null)c=go.AddComponent<T>();return c;}
    static GameObject Child(Transform p,string name) {var t=p.Find(name);var g=t!=null?t.gameObject:new GameObject(name);g.transform.SetParent(p,false);return g;}
    static GameObject Find(string old,string newer) => GameObject.Find(newer) ?? GameObject.Find(old);
    static void Edit(UnityEngine.Object o,Action<SerializedObject> f) {var s=new SerializedObject(o);f(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    static void Ref(UnityEngine.Object o,string f,UnityEngine.Object v) => Edit(o,s=>s.FindProperty(f).objectReferenceValue=v);
    static void Refs(UnityEngine.Object o,string f,UnityEngine.Object[] v) => Edit(o,s=>{var p=s.FindProperty(f);p.arraySize=v.Length;for(int i=0;i<v.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=v[i];});
    static Material Mat(string name,Color color) {var path=Art+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.1f);EditorUtility.SetDirty(m);return m;}
    static GameObject Shape(Transform p,string name,Vector3 pos,Vector3 scale,Material m,bool collider=false)
    {var t=p.Find(name);var g=t!=null?t.gameObject:GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;var col=g.GetComponent<Collider>();if(!collider && col!=null)UnityEngine.Object.DestroyImmediate(col);else if(collider && col==null)g.AddComponent<BoxCollider>();return g;}
    static void LabelWorld(Transform p,string name,string words,Vector3 pos,float size=2.2f)
    {var g=Child(p,name);g.transform.localPosition=pos;var text=C<TextMeshPro>(g);text.font=TMP_Settings.defaultFontAsset;text.text=words;text.fontSize=size;text.alignment=TextAlignmentOptions.Center;text.color=new Color(.07f,.17f,.20f);text.rectTransform.sizeDelta=new Vector2(8,2);}
    static void UI(GameObject g,Transform p,Vector2 pos,Vector2 size,Vector2 anchor)
    {g.transform.SetParent(p,false);var r=C<RectTransform>(g);r.anchorMin=r.anchorMax=anchor;r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=Vector3.one;}
    static GameObject Panel(Transform p,string name,Vector2 pos,Vector2 size,Vector2 anchor)
    {var t=p.Find(name);var g=t!=null?t.gameObject:new GameObject(name,typeof(RectTransform),typeof(Image));UI(g,p,pos,size,anchor);var image=C<Image>(g);image.color=new Color(.025f,.065f,.085f,.99f);return g;}
    static TMP_Text Text(Transform p,string name,string words,Vector2 pos,Vector2 size,float font,Vector2 anchor)
    {var t=p.Find(name);var g=t!=null?t.gameObject:new GameObject(name,typeof(RectTransform));UI(g,p,pos,size,anchor);var text=C<TextMeshProUGUI>(g);text.font=TMP_Settings.defaultFontAsset;text.text=words;text.fontSize=font;text.color=new Color(.94f,.98f,1);text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;return text;}
    static Button Button(Transform p,string name,Vector2 pos,Vector2 size,string words,out TMP_Text label)
    {var g=Panel(p,name,pos,size,new Vector2(.5f,1));g.GetComponent<Image>().color=new Color(.10f,.22f,.25f);var b=C<Button>(g);b.targetGraphic=g.GetComponent<Image>();var colors=b.colors;colors.highlightedColor=new Color(.50f,.85f,.78f);colors.selectedColor=colors.highlightedColor;b.colors=colors;label=Text(g.transform,"Text",words,Vector2.zero,size-new Vector2(35,4),23,new Vector2(.5f,.5f));label.alignment=TextAlignmentOptions.Center;return b;}
    static void Conversation(GameObject go,params string[] entries)
    {var npc=C<MissionDialogueNPC>(go);Edit(npc,s=>{var list=s.FindProperty("conversations");list.arraySize=entries.Length;for(int i=0;i<entries.Length;i++){var f=entries[i].Split('|');var p=list.GetArrayElementAtIndex(i);p.FindPropertyRelative("quest").objectReferenceValue=Q(f[0]);p.FindPropertyRelative("objectiveId").stringValue=f[1];p.FindPropertyRelative("requiredObjectiveId").stringValue=f[2];p.FindPropertyRelative("dialogue").objectReferenceValue=Load<DialogueData>(f[3]);p.FindPropertyRelative("activity").objectReferenceValue=f.Length>4?Load<NarrativeActivityData>(f[4]):null;}});}
    static void Identity(GameObject go,string name)
    {var text=go.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t=>t.name=="CharacterName");if(text!=null)text.text=name;var npc=go.GetComponent<MissionDialogueNPC>();if(npc!=null){var data=new SerializedObject(npc).FindProperty("npcData").objectReferenceValue as NPCData;if(data!=null){data.npcName=name;EditorUtility.SetDirty(data);}}}
    static void Pose(SerializedProperty p,int chapter,Transform actor,Vector3 position,float yaw)
    {p.FindPropertyRelative("chapter").intValue=chapter;p.FindPropertyRelative("actor").objectReferenceValue=actor;p.FindPropertyRelative("position").vector3Value=position;p.FindPropertyRelative("yaw").floatValue=yaw;}
    static void ToggleOutline(Transform parent)
    {var outline=Panel(parent,"BoxOutline",new Vector2(14,0),new Vector2(36,36),new Vector2(0,.5f));outline.GetComponent<Image>().color=new Color(.33f,.48f,.50f);outline.GetComponent<Image>().raycastTarget=false;outline.transform.SetAsFirstSibling();var label=parent.Find("Label")?.GetComponent<TMP_Text>();if(label!=null)label.alignment=TextAlignmentOptions.MidlineLeft;}
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        AssetDatabase.DesiredWorkerCount=1;AssetDatabase.ForceToDesiredWorkerCount();
        var sequence=UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();if(sequence==null)throw new Exception("Open Level01");
        var env=GameObject.Find("_Level/Environtment");var player=GameObject.Find("Player");
        var denis=Find("_Level/StoryCharacters/Dimas","_Level/StoryCharacters/Denis");denis.name="Denis";
        var aji=Find("_Level/StoryCharacters/Arga","_Level/StoryCharacters/Aji");aji.name="Aji";
        var ari=Find("_Level/StoryCharacters/Nara","_Level/StoryCharacters/Ari");ari.name="Ari";
        var teacher=Find("_Level/BuNadia","_Level/PakBambang");teacher.name="PakBambang";
        var billy=GameObject.Find("_Level/BullyGroup/Bully_Leader");
        Identity(denis,"Denis");Identity(aji,"Aji");Identity(ari,"Ari");Identity(teacher,"Pak Bambang");Identity(billy,"Billy");
        Conversation(denis,"ch01_friend|listen_friend||Dimas_Listen","ch01_route_support|route||Aji_Route_Support","ch01_route_provoke|repair|confront|Aji_Route_Repair","ch01_route_ignore|repair|witness|Aji_Route_Return","book|accept_request||Raka_Request","book|return_notebook|find_notebook|Raka_Return","ch01_extortion|hear_extortion||Dimas_Extortion","ch01_escort|agree_escort||Dimas_Escort","ch01_escort|check_in|reach_safe_zone|Dimas_Safe","ch03_repair|ask_consent||Bully_Consent","ch05_consent|confirm_boundaries||Together_Boundaries");
        Conversation(ari,"ch01_route_ignore|witness||Aji_Route_Ari","ch01_witness|speak_witness||Nara_Witness","ch02_witness|witness||Denis_Ari","ch04_support|preserve_evidence||Park_Evidence");
        Conversation(aji,"ch04_check|park_check||Park_Check","ch04_boundaries|state_boundary||Park_Boundary","ch05_facts|verify_facts||Together_Facts");
        Conversation(teacher,"ch01_extortion|seek_support|hear_extortion|Teacher_Extortion","ch01_witness|plan_support|speak_witness|Teacher_Plan","ch02_arithmetic|solve||Denis_Math_Intro|Activity_DenisMath","ch03_accountability|own_actions||Bully_Accountability","ch03_repair|make_plan|ask_consent|Bully_Plan","ch04_support|seek_support|preserve_evidence|Park_Support","ch05_action|follow_up|check_commitment|Together_FollowUp");
        Conversation(billy,"ch01_route_provoke|confront||Aji_Route_Provoke","ch02_pressure|hear_threat||DenisPressure_Threat","ch05_action|check_commitment||Together_Commitment");
        var vendor=Child(GameObject.Find("_Level/StoryCharacters").transform,"KantinVendor");vendor.transform.position=new Vector3(18,.05f,7.4f);
        var npcData=Load<NPCData>("NPC_BuSari");if(npcData==null){npcData=ScriptableObject.CreateInstance<NPCData>();AssetDatabase.CreateAsset(npcData,Data+"NPC_BuSari.asset");}npcData.npcName="Bu Sari";npcData.interactionPrompt="Beli makanan / bicara";EditorUtility.SetDirty(npcData);
        vendor.layer=2;var col=C<CapsuleCollider>(vendor);col.center=Vector3.up;col.height=2;col.radius=.4f;col.isTrigger=true;Ref(C<MissionDialogueNPC>(vendor),"npcData",npcData);
        var vendorTrigger=C<SphereCollider>(Child(vendor.transform,"InteractionRange"));vendorTrigger.radius=2.7f;vendorTrigger.center=Vector3.up;vendorTrigger.isTrigger=true;
        var vendorStand=Child(vendor.transform,"PlayerDialogueSpawnPoint");vendorStand.transform.localPosition=new Vector3(0,0,-2.7f);Ref(vendor.GetComponent<MissionDialogueNPC>(),"dialogueSpawnPoint",vendorStand.transform);
        Shape(vendor.transform,"TemporaryVendor",Vector3.up,new Vector3(.5f,1.8f,.5f),Mat("VendorApron",new Color(.72f,.52f,.32f)));LabelWorld(vendor.transform,"CharacterName","Bu Sari",Vector3.up*2.35f);C<WorldCharacterLabel>(vendor.transform.Find("CharacterName").gameObject);
        Conversation(vendor,"ch02_lunch|encounter||Denis_Lunch_Intro|Activity_DenisFood");
        // Keep character models untouched. Four roles share the existing controller.
        var pov=UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>();Edit(pov,s=>{var slots=s.FindProperty("perspectives");var spawn=slots.GetArrayElementAtIndex(0).FindPropertyRelative("spawn").objectReferenceValue;var material=AssetDatabase.LoadAssetAtPath<Material>(Art+"UniformArga.mat");slots.arraySize=4;for(int i=0;i<4;i++){var slot=slots.GetArrayElementAtIndex(i);slot.FindPropertyRelative("perspective").enumValueIndex=i;slot.FindPropertyRelative("npcProxy").objectReferenceValue=i==0?denis:i==1?billy:i==2?ari:aji;if(i==0){slot.FindPropertyRelative("uniformMaterial").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Material>(Art+"UniformDimas.mat");}if(i==3){slot.FindPropertyRelative("spawn").objectReferenceValue=spawn;slot.FindPropertyRelative("uniformMaterial").objectReferenceValue=material;}}});
        var ground=env.transform.Find("Ground");ground.localScale=new Vector3(6.4f,1,5.6f);
        var wall=Mat("SchoolWarmWalls",new Color(.83f,.80f,.69f));var wood=Mat("Wood",new Color(.36f,.24f,.17f));var chalk=Mat("Chalkboard",new Color(.12f,.26f,.23f));var pale=Mat("PathStone",new Color(.73f,.75f,.67f));var green=Mat("ExtendedGreen",new Color(.26f,.43f,.28f));
        // Expand existing perimeter, preserving its collision components.
        foreach(var t in env.GetComponentsInChildren<Transform>(true).Where(t=>t.name.EndsWith("Boundary") || t.name.StartsWith("Fence")))
        {var p=t.localPosition;var scale=t.localScale;if(Mathf.Abs(p.x)>Mathf.Abs(p.z)){p.x=Mathf.Sign(p.x)*32;scale.z=56;}else{p.z=Mathf.Sign(p.z)*28;scale.x=64;}t.localPosition=p;t.localScale=scale;}
        var school=env.transform.Find("SchoolBackdrop");if(school!=null)school.localPosition=new Vector3(0,0,9);
        var expanded=Child(env.transform,"ExpandedCampus");
        Shape(expanded.transform,"MainWalk",new Vector3(0,.014f,0),new Vector3(5,.025f,48),pale);
        Shape(expanded.transform,"CrossWalk",new Vector3(0,.017f,7),new Vector3(54,.025f,3.5f),pale);
        for(int i=0;i<12;i++){float x=(i%2==0?-1:1)*(23+(i%3)*2);float z=-22+(i/2)*8;Shape(expanded.transform,"Planter_"+i,new Vector3(x,.28f,z),new Vector3(2.8f,.5f,2),wall);Shape(expanded.transform,"Hedge_"+i,new Vector3(x,1,z),new Vector3(2.5f,1.2f,1.7f),green);}
        for(int i=0;i<6;i++) {float x=-24+i*9;Shape(expanded.transform,"BenchSeat_"+i,new Vector3(x,.48f,-16),new Vector3(3,.22f,.75f),wood);Shape(expanded.transform,"BenchBack_"+i,new Vector3(x,.9f,-16.3f),new Vector3(3,.9f,.15f),wood);}
        var classroom=Child(expanded.transform,"Classroom");classroom.transform.localPosition=new Vector3(-20,0,15);
        Shape(classroom.transform,"Floor",new Vector3(0,.018f,0),new Vector3(12,.035f,10),pale);
        Shape(classroom.transform,"BackWall",new Vector3(0,1.8f,5),new Vector3(12,3.6f,.2f),wall,true);
        Shape(classroom.transform,"LeftWall",new Vector3(-6,1.8f,0),new Vector3(.2f,3.6f,10),wall,true);
        Shape(classroom.transform,"RightWall",new Vector3(6,1.8f,0),new Vector3(.2f,3.6f,10),wall,true);
        Shape(classroom.transform,"FrontLeft",new Vector3(-4.3f,1.8f,-5),new Vector3(3.4f,3.6f,.2f),wall,true);
        Shape(classroom.transform,"FrontRight",new Vector3(4.3f,1.8f,-5),new Vector3(3.4f,3.6f,.2f),wall,true);
        Shape(classroom.transform,"DoorHeader",new Vector3(0,3.1f,-5),new Vector3(5.2f,1,.2f),wall,true);
        Shape(classroom.transform,"Roof",new Vector3(0,3.7f,0),new Vector3(12.4f,.2f,10.4f),wall);
        Shape(classroom.transform,"Board",new Vector3(0,2.1f,4.84f),new Vector3(5,1.8f,.12f),chalk);
        LabelWorld(classroom.transform,"BoardText","7 × 6 + 8 = ?\nPerkalian dahulu",new Vector3(0,2.1f,4.75f),1.65f);var boardText=classroom.transform.Find("BoardText").GetComponent<TMP_Text>();boardText.color=Color.white;boardText.rectTransform.sizeDelta=new Vector2(4.8f,1.5f);
        var boardMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"BoardTextWorld.mat");if(boardMaterial==null){boardMaterial=new Material(boardText.fontSharedMaterial);AssetDatabase.CreateAsset(boardMaterial,Art+"BoardTextWorld.mat");}boardMaterial.renderQueue=3000;if(boardMaterial.HasProperty("_ZTestMode"))boardMaterial.SetFloat("_ZTestMode",4);boardText.fontSharedMaterial=boardMaterial;EditorUtility.SetDirty(boardMaterial);
        for(int i=0;i<6;i++){float x=i%2==0?-2.9f:2.9f;float z=-2+(i/2)*1.7f;Shape(classroom.transform,"Desk_"+i,new Vector3(x,.7f,z),new Vector3(1.8f,.2f,.8f),wood);Shape(classroom.transform,"Chair_"+i,new Vector3(x,.45f,z-1),new Vector3(.7f,.8f,.7f),wood);}
        LabelWorld(expanded.transform,"ClassroomSign","RUANG KELAS\nPak Bambang",new Vector3(-20,3,9.75f),2.6f);
        var stall=Child(expanded.transform,"Canteen");stall.transform.localPosition=new Vector3(18,0,6.5f);
        Shape(stall.transform,"Counter",new Vector3(0,.75f,0),new Vector3(5,1.5f,1),wood,true);Shape(stall.transform,"Canopy",new Vector3(0,3.1f,0),new Vector3(6,.22f,3),green);LabelWorld(stall.transform,"CanteenSign","KANTIN • Bu Sari",new Vector3(0,2.5f,-.7f));
        Shape(stall.transform,"BreadTray",new Vector3(0,1.6f,-.1f),new Vector3(1,.2f,.6f),Mat("Bread",new Color(.86f,.67f,.38f)));
        var safe=UnityEngine.Object.FindAnyObjectByType<EscortCompanion>().Destination;safe.position=new Vector3(-19,.025f,-8);
        var layout=C<ChapterActorLayout>(sequence.gameObject);var members=GameObject.Find("_Level/BullyGroup").GetComponentsInChildren<BullyNPC>().Where(b=>b.gameObject!=billy).Select(b=>b.transform).ToArray();
        Edit(layout,s=>{var poses=s.FindProperty("poses");poses.arraySize=12;Pose(poses.GetArrayElementAtIndex(0),1,denis.transform,new Vector3(-7,.05f,5),180);Pose(poses.GetArrayElementAtIndex(1),1,ari.transform,new Vector3(-12,.05f,-4),90);Pose(poses.GetArrayElementAtIndex(2),1,teacher.transform,new Vector3(-14,.05f,8),180);Pose(poses.GetArrayElementAtIndex(3),1,billy.transform,new Vector3(8,.05f,6),180);Pose(poses.GetArrayElementAtIndex(4),1,members[0],new Vector3(10,.05f,7),180);Pose(poses.GetArrayElementAtIndex(5),1,members[1],new Vector3(11.5f,.05f,5),180);Pose(poses.GetArrayElementAtIndex(6),2,teacher.transform,new Vector3(-20,.05f,17),180);Pose(poses.GetArrayElementAtIndex(7),2,billy.transform,new Vector3(11,.05f,2),180);Pose(poses.GetArrayElementAtIndex(8),2,members[0],new Vector3(13,.05f,3),180);Pose(poses.GetArrayElementAtIndex(9),2,members[1],new Vector3(13,.05f,1),180);Pose(poses.GetArrayElementAtIndex(10),2,ari.transform,new Vector3(-8,.05f,-14),0);Pose(poses.GetArrayElementAtIndex(11),2,aji.transform,new Vector3(-3,.05f,5),180);});layout.Apply(1);
        // Stable authored request / encounter / random-rehide anchors across the larger area.
        var notebook=UnityEngine.Object.FindAnyObjectByType<QuestItem>();notebook.transform.position=new Vector3(7,.35f,-8);
        var hide=GameObject.Find("_Level/NotebookHidePoints");if(hide!=null){var points=new[]{new Vector3(15,.35f,-12),new Vector3(-13,.35f,-13),new Vector3(22,.35f,14),new Vector3(3,.35f,19)};for(int i=0;i<hide.transform.childCount;i++)hide.transform.GetChild(i).position=points[i%points.Length];}
        var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();Edit(opening,s=>{var list=s.FindProperty("speakers");foreach(var p in new[]{new{n="Aji",a=aji.transform},new{n="Denis",a=denis.transform},new{n="Billy",a=billy.transform},new{n="Ari",a=ari.transform},new{n="Pak Bambang",a=teacher.transform}}){for(int i=0;i<list.arraySize;i++){var e=list.GetArrayElementAtIndex(i);var old=e.FindPropertyRelative("speaker").stringValue;if(old==p.n || old==(p.n=="Aji"?"Arga":p.n=="Denis"?"Dimas":p.n=="Billy"?"Raka":p.n=="Ari"?"Nara":"Bu Nadia")){e.FindPropertyRelative("speaker").stringValue=p.n;e.FindPropertyRelative("actor").objectReferenceValue=p.a;}}}});
        var controls=new UnityEngine.Object[]{player.GetComponent<BullyingGame.Player.PlayerMovement>(),player.GetComponent<BullyingGame.Player.PlayerInputHandler>(),player.GetComponent<BullyingGame.Interaction.PlayerInteraction>(),GameObject.Find("Cameras/CinemachineCamera").GetComponent<CinemachineInputAxisController>()};
        var story=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>();var parent=story.transform.Find("SafeArea") ?? story.transform;
        var activityGO=Child(sequence.transform,"NarrativeActivities");var director=C<NarrativeActivityDirector>(activityGO);Ref(director,"player",player.transform);Refs(director,"gang",new UnityEngine.Object[]{billy.transform,members[0],members[1]});Refs(director,"controls",controls);
        var stage=Child(GameObject.Find("_Level").transform,"FoodEncounterStage");stage.transform.position=new Vector3(15,.05f,.5f);stage.transform.rotation=Quaternion.identity;Ref(director,"confrontationStage",stage.transform);
        var foodMaterial=Mat("Bread",new Color(.86f,.67f,.38f));var held=Shape(player.transform,"NarrativeFood",new Vector3(.38f,1.05f,.35f),new Vector3(.24f,.16f,.18f),foodMaterial);held.SetActive(false);Ref(director,"playerFood",held);
        var taken=Shape(billy.transform,"TakenFood",new Vector3(.38f,1.05f,.35f),new Vector3(.24f,.16f,.18f),foodMaterial);taken.SetActive(false);Ref(director,"takenFood",taken);
        var cameraGO=Child(GameObject.Find("Cameras").transform,"NarrativeActivityCamera");var cam=C<CinemachineCamera>(cameraGO);cam.Priority=0;Ref(director,"activityCamera",cam);
        var indoorGO=Child(GameObject.Find("Cameras").transform,"ClassroomGameplayCamera");C<CinemachineCamera>(indoorGO).Priority=0;var indoor=C<BullyingGame.Camera.IndoorCameraZone>(indoorGO);Ref(indoor,"player",player.transform);Edit(indoor,s=>s.FindProperty("room").boundsValue=new Bounds(new Vector3(-20,1.8f,15),new Vector3(11.6f,3.6f,9.6f)));
        var mathPanel=Panel(parent,"ArithmeticPanel",new Vector2(0,35),new Vector2(960,440),new Vector2(.5f,0));Ref(director,"mathPanel",mathPanel);Ref(director,"instruction",Text(mathPanel.transform,"Instruction","",new Vector2(0,-25),new Vector2(890,150),27,new Vector2(.5f,1)));
        var answers=new Button[3];var labels=new TMP_Text[3];for(int i=0;i<3;i++)answers[i]=Button(mathPanel.transform,"Answer"+i,new Vector2(0,-190-i*72),new Vector2(800,62),"",out labels[i]);Refs(director,"answerButtons",answers);Refs(director,"answerLabels",labels);mathPanel.SetActive(false);
        var choicePanel=Panel(parent,"AjiChoices",new Vector2(0,290),new Vector2(1430,325),new Vector2(.5f,0));Text(choicePanel.transform,"Heading","AJI • Pilih responsmu. Gunakan mouse, sentuh, atau navigasi UI dan konfirmasi.",new Vector2(0,-14),new Vector2(1380,45),23,new Vector2(.5f,1));var buttons=new Button[3];var choiceLabels=new TMP_Text[3];for(int i=0;i<3;i++)buttons[i]=Button(choicePanel.transform,"Choice"+i,new Vector2(0,-75-i*77),new Vector2(1360,67),"",out choiceLabels[i]);var choiceUI=C<DialogueChoiceUI>(story.gameObject);Ref(choiceUI,"panel",choicePanel);Refs(choiceUI,"buttons",buttons);Refs(choiceUI,"labels",choiceLabels);choicePanel.SetActive(false);
        var guide=C<StoryLearningHint>(story.gameObject);Ref(guide,"sequence",sequence);Ref(guide,"text",Text(parent,"LearningHint","",new Vector2(0,78),new Vector2(1050,65),21,new Vector2(.5f,0)));
        var nav=story.GetComponent<MissionNavigationHint>();Refs(nav,"characters",UnityEngine.Object.FindObjectsByType<MissionDialogueNPC>(FindObjectsInactive.Include));
        var pressureUI=C<NarrativePressureUI>(story.gameObject);Ref(pressureUI,"activity",director);Refs(pressureUI,"pressureDialogues",new UnityEngine.Object[]{Load<DialogueData>("DenisPressure_MathCorrect"),Load<DialogueData>("DenisPressure_MathWrong"),Load<DialogueData>("DenisPressure_FoodEffort"),Load<DialogueData>("DenisPressure_FoodTimeout"),Load<DialogueData>("DenisPressure_Threat")});
        var vignette=Panel(parent,"PressureVignette",Vector2.zero,new Vector2(1920,1080),new Vector2(.5f,.5f));var image=vignette.GetComponent<Image>();image.raycastTarget=false;image.color=Color.clear;image.sprite=Vignette();vignette.transform.SetAsFirstSibling();Ref(pressureUI,"vignette",image);
        var sound=C<AudioSource>(Child(story.transform,"PressureHeartbeat"));sound.playOnAwake=false;sound.loop=true;sound.spatialBlend=0;sound.clip=Heartbeat();sound.volume=0;Ref(pressureUI,"heartbeat",sound);
        var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();var pausePanel=(GameObject)new SerializedObject(pause).FindProperty("pausePanel").objectReferenceValue;
        var comfort=Panel(pausePanel.transform,"ComfortSettings",new Vector2(580,0),new Vector2(540,330),new Vector2(1,.5f));Text(comfort.transform,"Heading","KENYAMANAN BERMAIN",new Vector2(0,-18),new Vector2(500,45),23,new Vector2(.5f,1));
        var preferences=C<PlayerAccessibility>(pause.gameObject);var prefs=new[]{"instantText","reducedPressure","assistedQTE"};var captions=new[]{"Teks dialog langsung","Matikan vignette dan bunyi tekanan","Bantuan QTE pemalakan makanan"};
        for(int i=0;i<3;i++){var go=Panel(comfort.transform,prefs[i],new Vector2(0,-80-i*77),new Vector2(500,65),new Vector2(.5f,1));go.GetComponent<Image>().color=new Color(.12f,.23f,.25f);var toggle=C<Toggle>(go);toggle.targetGraphic=go.GetComponent<Image>();var mark=Panel(go.transform,"Check",new Vector2(18,0),new Vector2(28,28),new Vector2(0,.5f));mark.GetComponent<Image>().color=new Color(.43f,.9f,.72f);toggle.graphic=mark.GetComponent<Image>();Text(go.transform,"Label",captions[i],new Vector2(58,0),new Vector2(425,60),21,new Vector2(0,.5f));Ref(preferences,prefs[i],toggle);}
        var surface=env.GetComponent<NavMeshSurface>();surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.layerMask=~(1<<2);surface.BuildNavMesh();if(surface.navMeshData!=null && !EditorUtility.IsPersistent(surface.navMeshData))AssetDatabase.CreateAsset(surface.navMeshData,AssetDatabase.GenerateUniqueAssetPath(Art+"NavMesh_ExpandedCampus.asset"));
        foreach(var toggle in comfort.GetComponentsInChildren<Toggle>(true))ToggleOutline(toggle.transform);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(player.scene);EditorSceneManager.SaveScene(player.scene);
        return "Saved campus 64 × 56 m, indoor classroom, canteen, authored NPC layouts, 4 POV mappings, 3 response UI, untimed arithmetic and neutral coercion QTE, steady pressure vignette/audio and guides.";
    }
    static Sprite Vignette()
    {
        string path=Art+"PressureVignette.asset";var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(sprite!=null)return sprite;
        var texture=new Texture2D(256,144,TextureFormat.RGBA32,false);texture.name="SteadyPressure";texture.wrapMode=TextureWrapMode.Clamp;
        for(int y=0;y<144;y++)for(int x=0;x<256;x++){float dx=Mathf.Abs((x-127.5f)/127.5f),dy=Mathf.Abs((y-71.5f)/71.5f);float a=Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Sqrt(dx*dx+dy*dy)-.45f)/.75f));texture.SetPixel(x,y,new Color(1,1,1,a));}texture.Apply();AssetDatabase.CreateAsset(texture,path);sprite=Sprite.Create(texture,new Rect(0,0,256,144),new Vector2(.5f,.5f));sprite.name="PressureVignette";AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
    }
    static AudioClip Heartbeat()
    {
        string path="Assets/_Game/Audio/Prototype/PressureHeartbeat.wav";
        if(!File.Exists(path))using(var w=new BinaryWriter(File.Open(path,FileMode.Create))){int rate=22050,count=rate*2;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);for(int i=0;i<count;i++){double t=(double)i/rate;double p=t%1;double beat=Math.Exp(-35*p)*Math.Sin(2*Math.PI*70*p)+(p>.18?Math.Exp(-40*(p-.18))*Math.Sin(2*Math.PI*60*(p-.18))*.65:0);w.Write((short)(beat*.22*32760));}}AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    public static string Menu()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");EditorSceneManager.OpenScene("Assets/_Game/Scenes/01_MainMenu/01_MainMenu.unity");var menu=UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>();var settings=(GameObject)new SerializedObject(menu).FindProperty("settingsPanel").objectReferenceValue;
        UI(settings,settings.transform.parent,Vector2.zero,new Vector2(1000,610),new Vector2(.5f,.5f));settings.transform.SetAsLastSibling();var background=settings.GetComponent<Image>();background.color=new Color(.025f,.065f,.085f,1);background.sprite=AssetDatabase.LoadAllAssetsAtPath(Art+"UIWhite.asset").OfType<Sprite>().FirstOrDefault();var access=C<PlayerAccessibility>(menu.gameObject);
        Text(settings.transform,"PreferencesTitle","PENGATURAN",new Vector2(0,-20),new Vector2(900,50),32,new Vector2(.5f,1));Text(settings.transform,"VolumeCaption","Volume keseluruhan",new Vector2(0,-88),new Vector2(820,38),24,new Vector2(.5f,1));
        var volume=(Slider)new SerializedObject(menu).FindProperty("volumeSlider").objectReferenceValue;UI(volume.gameObject,settings.transform,new Vector2(0,-145),new Vector2(760,28),new Vector2(.5f,1));
        foreach(var child in volume.GetComponentsInChildren<RectTransform>(true).Where(t=>t!=volume.transform))child.localScale=Vector3.one;
        foreach(var area in new[]{volume.fillRect!=null?volume.fillRect.parent as RectTransform:null,volume.handleRect!=null?volume.handleRect.parent as RectTransform:null})if(area!=null&&area!=volume.transform){area.anchorMin=Vector2.zero;area.anchorMax=Vector2.one;area.offsetMin=new Vector2(15,0);area.offsetMax=new Vector2(-15,0);}
        if(volume.fillRect!=null){volume.fillRect.offsetMin=volume.fillRect.offsetMax=Vector2.zero;volume.fillRect.anchorMin=Vector2.zero;volume.fillRect.anchorMax=Vector2.one;}
        if(volume.handleRect!=null)volume.handleRect.sizeDelta=new Vector2(26,42);
        var sliderBackground=volume.transform.Find("Background") as RectTransform;if(sliderBackground!=null){sliderBackground.anchorMin=Vector2.zero;sliderBackground.anchorMax=Vector2.one;sliderBackground.offsetMin=sliderBackground.offsetMax=Vector2.zero;}
        var names=new[]{"InstantText","ReducedPressure","AssistedQTE"};var words=new[]{"Tampilkan teks dialog langsung","Matikan vignette dan bunyi tekanan","Bantuan QTE — lewati usaha pada adegan pemalakan"};
        for(int i=0;i<3;i++){var go=Panel(settings.transform,names[i],new Vector2(0,-205-i*90),new Vector2(900,72),new Vector2(.5f,1));go.GetComponent<Image>().color=new Color(.12f,.23f,.25f);var toggle=C<Toggle>(go);toggle.targetGraphic=go.GetComponent<Image>();var mark=Panel(go.transform,"Check",new Vector2(18,0),new Vector2(30,30),new Vector2(0,.5f));mark.GetComponent<Image>().color=new Color(.43f,.9f,.72f);toggle.graphic=mark.GetComponent<Image>();Text(go.transform,"Label",words[i],new Vector2(65,0),new Vector2(810,65),22,new Vector2(0,.5f));Ref(access,i==0?"instantText":i==1?"reducedPressure":"assistedQTE",toggle);}
        foreach(var toggle in settings.GetComponentsInChildren<Toggle>(true))ToggleOutline(toggle.transform);
        TMP_Text closeLabel;var close=Button(settings.transform,"ClosePreferences",new Vector2(0,-520),new Vector2(900,55),"Kembali ke menu",out closeLabel);while(close.onClick.GetPersistentEventCount()>0)UnityEditor.Events.UnityEventTools.RemovePersistentListener(close.onClick,0);UnityEditor.Events.UnityEventTools.AddPersistentListener(close.onClick,menu.ToggleSettings);
        foreach(var text in menu.GetComponentsInChildren<TMP_Text>(true))
        {
            if(text.text.Contains("Jalani kisah Arga"))text.text="Ikuti Aji mengenal Denis, Ari, dan Billy.\nDengarkan kebutuhan, hormati batas, dan lihat dampak tindakan dari sisi mereka.";
            if(text.text=="Pengaturan suara")text.text="Pengaturan";
        }
        EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);EditorSceneManager.SaveScene(menu.gameObject.scene);EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Level_01/Level01.unity");return "Saved accessibility preferences in MainMenu settings and restored Level01.";
    }
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using Unity.Cinemachine;
using Unity.AI.Navigation;
using TMPro;
using BullyingGame.Core;
using BullyingGame.NPC;
using BullyingGame.Quest;
using BullyingGame.Dialogue;
using BullyingGame.Cinematics;
using BullyingGame.Events;
using BullyingGame.Save;
using BullyingGame.UI;
using static BullyingGame.EditorTools.PRDContentAuthoring;

namespace BullyingGame.EditorTools
{
    public static class PRDSceneAuthoring
    {
        const string BaseScene="Assets/_Game/Scenes/Levels/Level_01/Level01.unity";
        const string Art="Assets/_Game/Data/PRD/";
        static T C<T>(GameObject g) where T:Component=>g.GetComponent<T>()??g.AddComponent<T>();
        static GameObject Child(Transform p,string name){var g=new GameObject(name);g.transform.SetParent(p,false);return g;}
        static T Field<T>(UnityEngine.Object o,string f) where T:UnityEngine.Object=>new SerializedObject(o).FindProperty(f).objectReferenceValue as T;
        static Material Mat(string name,Color color)
        {
            var path=Art+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;EditorUtility.SetDirty(m);return m;
        }
        static GameObject Cube(Transform p,string n,Vector3 pos,Vector3 scale,Material mat,bool collider=true)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
        }
        static void Sign(Transform p,string name,string text,Vector3 pos,float width=7)
        {
            var g=Child(p,name);g.transform.localPosition=pos;g.transform.rotation=Quaternion.Euler(0,0,0);var t=g.AddComponent<TextMeshPro>();t.font=TMP_Settings.defaultFontAsset;t.text=text;t.fontSize=3.1f;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(width,1);t.color=new Color(.08f,.17f,.2f);
        }
        static void Room(Transform p,string name,Vector3 pos,Material wall,Material floor,Material furniture)
        {
            var r=Child(p,name);r.transform.position=pos;
            Cube(r.transform,"Floor",new Vector3(0,.015f,0),new Vector3(12,.03f,12),floor);
            Cube(r.transform,"BackWall",new Vector3(0,1.7f,6),new Vector3(12,3.4f,.2f),wall);
            Cube(r.transform,"LeftWall",new Vector3(-6,1.7f,0),new Vector3(.2f,3.4f,12),wall);
            Cube(r.transform,"RightWall",new Vector3(6,1.7f,0),new Vector3(.2f,3.4f,12),wall);
            Cube(r.transform,"FrontLeft",new Vector3(-4.3f,1.7f,-6),new Vector3(3.4f,3.4f,.2f),wall);
            Cube(r.transform,"FrontRight",new Vector3(4.3f,1.7f,-6),new Vector3(3.4f,3.4f,.2f),wall);
            Cube(r.transform,"Header",new Vector3(0,3.1f,-6),new Vector3(5.2f,.6f,.2f),wall);
            Cube(r.transform,"Desk",new Vector3(-3,.8f,3),new Vector3(3,.2f,1.3f),furniture);
            Cube(r.transform,"Cabinet",new Vector3(4,1,4.8f),new Vector3(2,2,.8f),furniture);
            for(int i=0;i<2;i++)Cube(r.transform,"Chair"+i,new Vector3(-4+i*2,.5f,1.7f),new Vector3(.8f,1,.8f),furniture);
            Sign(r.transform,"RoomName",name,new Vector3(0,3.8f,-6.15f));
        }
        static GameObject Environment(GameObject classroom)
        {
            var root=new GameObject("SchoolEnvironment");var ground=Mat("SchoolGrass",new Color(.35f,.51f,.40f));var wall=Mat("SchoolWall",new Color(.82f,.86f,.83f));var stone=Mat("SchoolPath",new Color(.59f,.63f,.61f));var wood=Mat("SchoolFurniture",new Color(.42f,.28f,.18f));var lines=Mat("FieldLines",new Color(.93f,.94f,.86f));
            Cube(root.transform,"Ground",new Vector3(0,-.15f,0),new Vector3(64,.3f,56),ground);
            Cube(root.transform,"WestBoundary",new Vector3(-32,1.5f,0),new Vector3(.3f,3,56),wall);Cube(root.transform,"EastBoundary",new Vector3(32,1.5f,0),new Vector3(.3f,3,56),wall);
            Cube(root.transform,"NorthBoundary",new Vector3(0,1.5f,28),new Vector3(64,3,.3f),wall);Cube(root.transform,"SouthBoundary",new Vector3(0,1.5f,-28),new Vector3(64,3,.3f),wall);
            Cube(root.transform,"Corridor",new Vector3(0,.015f,7),new Vector3(60,.03f,4),stone);
            Cube(root.transform,"QuietRouteNorth",new Vector3(6,.015f,25),new Vector3(49,.03f,3),stone);
            Cube(root.transform,"QuietRouteSide",new Vector3(-10,.015f,16),new Vector3(3,.03f,19),stone);
            Cube(root.transform,"QuietRouteEast",new Vector3(29,.015f,16),new Vector3(3,.03f,19),stone);
            classroom.name="Kelas";classroom.transform.SetParent(root.transform,true);classroom.transform.position=new Vector3(-20,0,16);
            var roof=classroom.transform.Find("Roof");if(roof!=null)UnityEngine.Object.DestroyImmediate(roof.gameObject);
            var board=classroom.transform.Find("BoardText");if(board!=null)board.GetComponent<TMP_Text>().text="Belajar bersama · saling menghormati";
            Sign(root.transform,"ClassroomLabel","KELAS",new Vector3(-20,3.8f,10.8f));
            Room(root.transform,"RUANG GURU",new Vector3(5,0,17),wall,stone,wood);Room(root.transform,"BK",new Vector3(21,0,17),wall,stone,wood);
            Cube(root.transform,"Field",new Vector3(0,.02f,-9),new Vector3(26,.04f,20),Mat("Court",new Color(.25f,.41f,.38f)));
            for(int i=-1;i<=1;i+=2){Cube(root.transform,"Sideline"+i,new Vector3(i*13,.06f,-9),new Vector3(.1f,.03f,20),lines,false);Cube(root.transform,"Endline"+i,new Vector3(0,.06f,-9+i*10),new Vector3(26,.03f,.1f),lines,false);}
            Cube(root.transform,"CenterLine",new Vector3(0,.06f,-9),new Vector3(26,.03f,.1f),lines,false);
            Cube(root.transform,"Bench",new Vector3(-17,.5f,-8),new Vector3(4,1,.8f),wood);
            Sign(root.transform,"FieldLabel","LAPANGAN",new Vector3(-5,2.7f,2));Sign(root.transform,"Guide","KELAS  ←     RUANG GURU  →     BK  →",new Vector3(-1,3.8f,8),18);
            Sign(root.transform,"QuietLabel","JALUR TENANG →",new Vector3(-10,2.8f,23),9);
            Cube(root.transform,"ClassCabinet",new Vector3(-24,1.2f,19.8f),new Vector3(1.4f,2.4f,.8f),wood);
            Cube(root.transform,"TeacherDesk",new Vector3(-20,.8f,19.5f),new Vector3(2.4f,.2f,1),wood);
            var surface=root.AddComponent<NavMeshSurface>();surface.collectObjects=CollectObjects.Children;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.BuildNavMesh();
            string navPath=Art+"SchoolNavMesh.asset";var existing=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if(existing==null)AssetDatabase.CreateAsset(surface.navMeshData,navPath);else{EditorUtility.CopySerialized(surface.navMeshData,existing);surface.navMeshData=existing;}
            string folder="Assets/_Game/Prefabs/Environment";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/_Game/Prefabs","Environment");
            PrefabUtility.SaveAsPrefabAssetAndConnect(root,folder+"/SchoolEnvironment.prefab",InteractionMode.AutomatedAction);return root;
        }
        static GameObject Actor(Transform parent,string id,Vector3 pos,GameObject template,bool adult=false)
        {
            var g=Child(parent,id);g.transform.position=pos;g.transform.rotation=Quaternion.Euler(0,180,0);
            var visual=UnityEngine.Object.Instantiate(template,g.transform);visual.name="RiggedVisual";visual.transform.localPosition=Vector3.zero;visual.SetActive(true);
            var head=visual.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Head");if(head!=null){head.localScale=Vector3.one*2.2f;head.localPosition=new Vector3(0,.28f,0);}
            var white=Mat("UniformWhite",new Color(.91f,.93f,.91f));var blue=Mat("UniformNavy",new Color(.075f,.13f,.26f));var gold=Mat("OSIS",new Color(.9f,.72f,.12f));var adultMat=Mat("TeacherUniform",new Color(.63f,.52f,.35f));
            foreach(var r in visual.GetComponentsInChildren<Renderer>())if(r.name.StartsWith("Uniform"))r.sharedMaterial=adult?adultMat:white;else if(r.name.Contains("Thigh")||r.name.Contains("Shin"))r.sharedMaterial=adult?adultMat:blue;
            var chest=visual.GetComponentsInChildren<Transform>().First(t=>t.name=="Chest");Cube(chest,"OSIS",new Vector3(-.15f,-.1f,.19f),new Vector3(.10f,.12f,.025f),gold,false);
            if(id=="Bima")visual.transform.localScale=new Vector3(1.2f,1,1.15f);
            var hair=head==null?null:head.Find("Hair");if(hair!=null&&id=="Dion")Cube(head,"Mullet",new Vector3(0,0,-.11f),new Vector3(.28f,.28f,.13f),hair.GetComponent<Renderer>().sharedMaterial,false);
            if(head!=null&&id=="Raka")for(int i=-1;i<=1;i+=2)Cube(head,"Glasses"+i,new Vector3(i*.075f,.03f,.17f),new Vector3(.11f,.08f,.025f),Mat("Glasses",Color.black),false);
            if(head!=null&&(id=="Sari"||id=="Laras"||id=="BuRatih"||id=="IbuAji"))Cube(head,"HairBack",new Vector3(0,-.02f,-.13f),new Vector3(.3f,.32f,.15f),Mat("HairDark",new Color(.08f,.065f,.06f)),false);
            var col=g.AddComponent<CapsuleCollider>();col.center=new Vector3(0,1.15f,0);col.height=2.3f;col.radius=.35f;
            var npc=g.AddComponent<MissionDialogueNPC>();Ref(npc,"npcData",Load<NPCData>("NPC_"+id));Ref(npc,"fallbackDialogue",Load<DialogueData>("Dialogue_Fallback_"+id));
            var stand=Child(g.transform,"PlayerDialogueSpawnPoint");stand.transform.localPosition=new Vector3(-1.3f,0,1.8f);Ref(npc,"dialogueSpawnPoint",stand.transform);
            return g;
        }
        static void Opening(int number,Dictionary<string,GameObject> cast,GameObject player)
        {
            var manager=UnityEngine.Object.FindAnyObjectByType<CutsceneManager>(FindObjectsInactive.Include);if(manager!=null)manager.gameObject.SetActive(true);
            var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>(FindObjectsInactive.Include);opening.gameObject.SetActive(true);var director=Field<PlayableDirector>(opening,"timeline");
            var timeline=ScriptableObject.CreateInstance<TimelineAsset>();string path=Art+"Opening0"+number+".playable";var existing=AssetDatabase.LoadAssetAtPath<TimelineAsset>(path);if(existing!=null){timeline=existing;foreach(var t in timeline.GetOutputTracks().ToArray())timeline.DeleteTrack(t);}else AssetDatabase.CreateAsset(timeline,path);
            director.playableAsset=timeline;director.playOnAwake=false;var receiver=C<SignalReceiver>(director.gameObject);
            var signal=Load<SignalAsset>("OpeningSignal");if(signal==null){signal=ScriptableObject.CreateInstance<SignalAsset>();AssetDatabase.CreateAsset(signal,Art+"OpeningSignal.asset");}
            for(int i=receiver.Count()-1;i>=0;i--)receiver.RemoveAtIndex(i);
            var reaction=new UnityEngine.Events.UnityEvent();UnityEventTools.AddPersistentListener(reaction,opening.StartOpeningDialogue);receiver.AddReaction(signal,reaction);
            var track=timeline.CreateTrack<CinemachineTrack>(null,"Story cameras");director.SetGenericBinding(track,UnityEngine.Object.FindAnyObjectByType<CinemachineBrain>());
            var root=GameObject.Find("PRDOpeningShots");if(root!=null)UnityEngine.Object.DestroyImmediate(root);root=new GameObject("PRDOpeningShots");
            Func<string,Vector3,Vector3,double,double,CinemachineCamera> shot=(name,pos,focus,start,duration)=>
            {
                var g=Child(root.transform,name);g.transform.SetPositionAndRotation(pos,Quaternion.LookRotation(focus-pos));var cam=g.AddComponent<CinemachineCamera>();cam.Priority=0;cam.Lens.FieldOfView=50;
                var clip=track.CreateClip<CinemachineShot>();clip.start=start;clip.duration=duration;clip.displayName=name;var a=(CinemachineShot)clip.asset;a.VirtualCamera.exposedName=new PropertyName("PRD_"+number+"_"+name);director.SetReferenceValue(a.VirtualCamera.exposedName,cam);return cam;
            };
            var wide=shot("Sekolah",new Vector3(-6,12,-22),new Vector3(0,0,8),0,3);
            var all=new Dictionary<string,Transform>{{"Aji",player.transform}};
            foreach(var pair in cast){var data=Load<NPCData>("NPC_"+pair.Key);if(data!=null&&pair.Key!="Koridor"&&pair.Key!="FollowUp")all[data.npcName]=pair.Value.transform;}
            Edit(opening,s=>{var speakers=s.FindProperty("speakers");speakers.arraySize=all.Count;int index=0;foreach(var pair in all){double time=3+index*2;var camera=shot(pair.Key.Replace(" ",""),pair.Value.position+new Vector3(1.3f,2.15f,-3.8f),pair.Value.position+Vector3.up*1.4f,time,2);var p=speakers.GetArrayElementAtIndex(index++);p.FindPropertyRelative("speaker").stringValue=pair.Key;p.FindPropertyRelative("actor").objectReferenceValue=pair.Value;p.FindPropertyRelative("shot").objectReferenceValue=camera;p.FindPropertyRelative("timelineTime").floatValue=(float)time+.2f;}s.FindProperty("actors").arraySize=0;s.FindProperty("stagePoints").arraySize=0;s.FindProperty("player").objectReferenceValue=player.transform;s.FindProperty("dialogueShot").objectReferenceValue=wide;s.FindProperty("skipSummary").objectReferenceValue=Load<DialogueData>("Dialogue_Skip0"+number);});
            shot("Penutup",new Vector3(-6,12,-22),new Vector3(0,0,8),3+all.Count*2,2);
            var signals=timeline.CreateTrack<SignalTrack>(null,"Read at your pace");var marker=signals.CreateMarker<SignalEmitter>(.7);marker.asset=signal;marker.emitOnce=true;director.SetGenericBinding(signals,receiver);EditorUtility.SetDirty(timeline);
        }
        static void Wire(StoryChapterSequence sequence,Dictionary<string,GameObject> cast,int chapter,GameObject player)
        {
            var source=Read();var all=Enumerable.Range(1,5).Select(i=>(UnityEngine.Object)Load<NarrativeChapterData>("Chapter0"+i)).ToArray();Refs(sequence,"chapters",all);
            Ref(sequence.GetComponent<SchoolStoryPresentation>(),"privateSummary",Load<DialogueData>("Dialogue_SkipM13"));
            Edit(sequence,s=>{s.FindProperty("sceneChapterNumber").intValue=chapter;s.FindProperty("outdoor").objectReferenceValue=null;});
            var layout=sequence.GetComponent<ChapterActorLayout>();if(layout!=null)Edit(layout,s=>s.FindProperty("poses").arraySize=0);
            var outdoor=sequence.GetComponent<OutdoorChapterPresentation>();if(outdoor!=null)outdoor.enabled=false;
            var pov=UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>();var spawn=GameObject.Find("PRDPlayerSpawn")??new GameObject("PRDPlayerSpawn");spawn.transform.position=chapter==1?new Vector3(-20,.05f,12):chapter==2?new Vector3(-10,.05f,-1):chapter==3?new Vector3(-20,.05f,12):new Vector3(0,.05f,5);
            Edit(pov,s=>{var p=s.FindProperty("perspectives");p.arraySize=1;var entry=p.GetArrayElementAtIndex(0);entry.FindPropertyRelative("perspective").enumValueIndex=(int)POVType.Protagonist;entry.FindPropertyRelative("spawn").objectReferenceValue=spawn.transform;entry.FindPropertyRelative("npcProxy").objectReferenceValue=null;entry.FindPropertyRelative("uniformMaterial").objectReferenceValue=null;});
            foreach(var pair in cast)
            {
                var npc=pair.Value.GetComponent<MissionDialogueNPC>();var entries=source.missions.Skip((chapter-1)*3).Take(3).SelectMany(m=>m.steps.Where(step=>step.actor==pair.Key).Select(step=>new{m,step})).ToArray();
                Edit(npc,s=>{var p=s.FindProperty("conversations");p.arraySize=entries.Length;for(int i=0;i<entries.Length;i++){var e=entries[i];var v=p.GetArrayElementAtIndex(i);v.FindPropertyRelative("quest").objectReferenceValue=Load<QuestData>("Quest_"+e.m.id);v.FindPropertyRelative("objectiveId").stringValue=e.step.id;v.FindPropertyRelative("requiredObjectiveId").stringValue=e.step.requires??"";v.FindPropertyRelative("dialogue").objectReferenceValue=Load<DialogueData>("Dialogue_"+e.m.id+"_"+e.step.id);v.FindPropertyRelative("activity").objectReferenceValue=null;}});
            }
            var journals=C<MissionJournal>(sequence.gameObject);Edit(journals,s=>{var entries=source.missions.SelectMany(m=>m.steps.Where(st=>!string.IsNullOrEmpty(st.journal)).Select(st=>new{m,st})).ToArray();var p=s.FindProperty("entries");p.arraySize=entries.Length;for(int i=0;i<entries.Length;i++){var e=entries[i];var v=p.GetArrayElementAtIndex(i);v.FindPropertyRelative("quest").objectReferenceValue=Load<QuestData>("Quest_"+e.m.id);v.FindPropertyRelative("objectiveId").stringValue=e.st.id;v.FindPropertyRelative("caseId").stringValue=e.st.caseId??"Sekolah";v.FindPropertyRelative("status").stringValue=e.st.status??"Pengamatan dan catatan";v.FindPropertyRelative("text").stringValue=e.st.journal;}});
            var navigators=UnityEngine.Object.FindObjectsByType<MissionNavigationHint>(FindObjectsInactive.Include,FindObjectsSortMode.None);var actors=cast.Values.Select(g=>(UnityEngine.Object)g.GetComponent<MissionDialogueNPC>()).ToArray();
            foreach(var nav in navigators){Refs(nav,"characters",actors);Ref(nav,"notebook",null);Ref(nav,"companion",null);}
            var guide=C<SchoolFieldGuide>(sequence.gameObject);Ref(guide,"sequence",sequence);Ref(guide,"player",player.transform);Refs(guide,"targets",actors);
            var companion=C<SchoolCompanion>(cast["Sari"]);Ref(companion,"sequence",sequence);Ref(companion,"player",player.transform);Ref(companion,"teacher",cast["BuRatih"].transform);Ref(companion,"orientation",Load<QuestData>("Quest_M01"));Ref(companion,"report",Load<QuestData>("Quest_M12"));
            cast["Sari"].transform.position=chapter==1?new Vector3(-20,.05f,15):new Vector3(-1,.05f,6);
            cast["FollowUp"].SetActive(chapter==5);cast["Koridor"].SetActive(true);
            Opening(chapter,cast,player);
            var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();
            Ref(sequence,"opening",opening);
            Ref(UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>(),"opening",opening);
            pov.Apply(POVType.Protagonist);
        }
        [MenuItem("Tools/The Unseen Side/Apply PRD 1.1 Scenes")]
        public static void Build()
        {
            if(Application.isPlaying)throw new Exception("Edit Mode required");
            string backup="Logs/BeforePRD11";Directory.CreateDirectory(backup);
            foreach(var s in EditorBuildSettings.scenes)if(File.Exists(s.path)&&!File.Exists(backup+"/"+Path.GetFileName(s.path)))File.Copy(s.path,backup+"/"+Path.GetFileName(s.path));
            PRDContentAuthoring.Build();EditorSceneManager.OpenScene(BaseScene);
            if(GameObject.Find("SchoolEnvironment")!=null){RefreshExisting();return;}
            var oldVisual=GameObject.Find("_Level/StoryCharacters/Denis/RiggedVisual");if(oldVisual==null)throw new Exception("Original prototype required; restore scene backup before rerunning full scene construction.");
            var template=UnityEngine.Object.Instantiate(oldVisual);template.name="PRDVisualTemplate";
            var classroom=UnityEngine.Object.Instantiate(GameObject.Find("_Level/Environtment/ExpandedCampus/Classroom"));
            UnityEngine.Object.DestroyImmediate(GameObject.Find("_Level"));Environment(classroom);
            var player=GameObject.Find("Player");foreach(Transform child in player.transform.Cast<Transform>().ToArray())if(child.name.Contains("Missing Prefab")||child.name=="NarrativeFood")UnityEngine.Object.DestroyImmediate(child.gameObject);
            foreach(var obj in new[]{"NotebookEventDirector","NotebookCinematic","QTEManager"}){var g=GameObject.Find("[GameSystem]/"+obj);if(g!=null)g.SetActive(false);}
            foreach(var c in UnityEngine.Object.FindObjectsByType<NarrativeActivityDirector>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.enabled=false;
            foreach(var c in UnityEngine.Object.FindObjectsByType<NarrativePressureUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.enabled=false;
            foreach(var c in UnityEngine.Object.FindObjectsByType<MobileGameplayUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))c.enabled=false;
            foreach(var c in UnityEngine.Object.FindObjectsByType<BullyingGame.Camera.IndoorCameraZone>(FindObjectsInactive.Include,FindObjectsSortMode.None)){c.GetComponent<CinemachineCamera>().Priority=0;c.enabled=false;}
            foreach(var cam in UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c.name!="CinemachineCamera"&&c.name!="DialogueCamera").ToArray())UnityEngine.Object.DestroyImmediate(cam.gameObject);
            var castRoot=new GameObject("PRDCast");var cast=new Dictionary<string,GameObject>();
            string[] ids={"Sari","Bima","Laras","Raka","Dion","Tio","Fajar","PakBeni","BuRatih","PakArif","AyahAji","IbuAji"};
            Vector3[] positions={new Vector3(-20,0,15),new Vector3(-16,0,-4),new Vector3(-20,0,17),new Vector3(-6,0,7),new Vector3(12,0,-4),new Vector3(14,0,-6),new Vector3(9,0,15),new Vector3(-8,0,-3),new Vector3(5,0,17),new Vector3(21,0,17),new Vector3(-23,0,9),new Vector3(-21,0,9)};
            for(int i=0;i<ids.Length;i++)cast[ids[i]]=Actor(castRoot.transform,ids[i],positions[i]+Vector3.up*.05f,template,i>=7);
            foreach(var id in new[]{"Koridor","FollowUp"})
            {
                var g=Cube(castRoot.transform,id,id=="Koridor"?new Vector3(-2,1,7):new Vector3(0,1,7),new Vector3(.6f,2,.35f),Mat("SignPost",new Color(.2f,.45f,.47f)));
                // Collider dimensions are kept in the transform; conversation needs a world-size anchor.
                var npc=g.AddComponent<MissionDialogueNPC>();Ref(npc,"npcData",Load<NPCData>("NPC_"+id));Ref(npc,"fallbackDialogue",Load<DialogueData>("Dialogue_Fallback_"+id));cast[id]=g;
            }
            var parents=new GameObject("PRDParents");cast["AyahAji"].transform.SetParent(parents.transform,true);cast["IbuAji"].transform.SetParent(parents.transform,true);
            var agent=cast["Sari"].AddComponent<NavMeshAgent>();agent.speed=3.2f;agent.radius=.32f;agent.height=2.3f;agent.stoppingDistance=.5f;
            var sequence=UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();var visual=player.transform.Find("RiggedVisual");visual.gameObject.SetActive(true);var head=visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Head");head.localScale=Vector3.one*2.2f;head.localPosition=new Vector3(0,.28f,0);
            foreach(var r in visual.GetComponentsInChildren<Renderer>())if(r.name.StartsWith("Uniform"))r.sharedMaterial=Mat("UniformWhite",new Color(.91f,.93f,.91f));else if(r.name.Contains("Shin")||r.name.Contains("Thigh"))r.sharedMaterial=Mat("UniformNavy",new Color(.075f,.13f,.26f));
            player.transform.Find("CameraTarget").localPosition=new Vector3(0,1.6f,0);var cc=player.GetComponent<CharacterController>();cc.height=2.4f;cc.center=new Vector3(0,1.2f,0);
            var body=player.GetComponent<Rigidbody>();if(body!=null){body.isKinematic=true;body.useGravity=false;}
            var bag=Cube(null,"RakaBag",new Vector3(9,.5f,15),new Vector3(.48f,.62f,.22f),Mat("BagBlue",new Color(.11f,.22f,.45f)),false);
            var pencils=Cube(null,"RakaPencils",new Vector3(-5.5f,.08f,6),new Vector3(.4f,.05f,.1f),Mat("Pencil",Color.yellow),false);
            var stage=new GameObject("PrivateFollowUpStage");stage.transform.position=new Vector3(110,0,0);Cube(stage.transform,"Floor",Vector3.zero,new Vector3(16,.1f,12),Mat("SchoolPath",new Color(.59f,.63f,.61f)));Cube(stage.transform,"Wall",new Vector3(0,2,5),new Vector3(16,4,.2f),Mat("SchoolWall",new Color(.82f,.86f,.83f)));
            for(int i=0;i<4;i++){string id=new[]{"Dion","Tio","Fajar","PakArif"}[i];var g=Actor(stage.transform,id,new Vector3(105+i*2.6f,.05f,1),template,i==3);g.GetComponent<MissionDialogueNPC>().enabled=false;}
            var privateCam=new GameObject("PrivateFollowUpCamera").AddComponent<CinemachineCamera>();privateCam.transform.SetPositionAndRotation(new Vector3(110,3,-8),Quaternion.LookRotation(new Vector3(109,1.2f,1)-new Vector3(110,3,-8)));privateCam.Priority=0;
            var presentation=C<SchoolStoryPresentation>(sequence.gameObject);Ref(presentation,"player",player.transform);Ref(presentation,"sari",cast["Sari"].transform);Ref(presentation,"ratih",cast["BuRatih"].transform);Ref(presentation,"raka",cast["Raka"].transform);Ref(presentation,"dion",cast["Dion"].transform);Ref(presentation,"tio",cast["Tio"].transform);Ref(presentation,"fajar",cast["Fajar"].transform);Ref(presentation,"bag",bag);Ref(presentation,"pencils",pencils);Ref(presentation,"privateStage",stage);Ref(presentation,"privateCamera",privateCam);Ref(presentation,"parents",parents);Ref(presentation,"bagMission",Load<QuestData>("Quest_M11"));Ref(presentation,"supportMission",Load<QuestData>("Quest_M03"));Ref(presentation,"routeMission",Load<QuestData>("Quest_M12"));stage.SetActive(false);
            UnityEngine.Object.DestroyImmediate(template);PolishUI();
            for(int chapter=1;chapter<=5;chapter++)
            {
                Wire(sequence,cast,chapter,player);
                string scenePath="Assets/_Game/Scenes/Levels/Level_0"+chapter+"/Level0"+chapter+".unity";
                EditorSceneManager.MarkSceneDirty(sequence.gameObject.scene);EditorSceneManager.SaveScene(sequence.gameObject.scene,scenePath,chapter!=1);
            }
            AssetDatabase.SaveAssets();Menu();Blockout();EditorSceneManager.OpenScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity");
        }
        static void Rect(RectTransform r,Vector2 pos,Vector2 size){r.anchoredPosition=pos;r.sizeDelta=size;}
        public static void RefreshExisting()
        {
            if(Application.isPlaying)throw new Exception("Edit Mode required");
            for(int chapter=1;chapter<=5;chapter++)
            {
                var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Level_0"+chapter+"/Level0"+chapter+".unity");
                var player=GameObject.Find("Player");player.transform.Find("RiggedVisual").gameObject.SetActive(true);
                foreach(var root in scene.GetRootGameObjects())foreach(var head in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Head"))head.localPosition=new Vector3(0,.28f,0);
                var cast=GameObject.Find("PRDCast").GetComponentsInChildren<MissionDialogueNPC>(true).ToDictionary(n=>n.name,n=>n.gameObject);
                foreach(var root in scene.GetRootGameObjects().Where(r=>r.name=="PRDParents"))
                    foreach(var npc in root.GetComponentsInChildren<MissionDialogueNPC>(true))cast[npc.name]=npc.gameObject;
                Wire(UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>(),cast,chapter,player);
                PolishUI();EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();Menu();EditorSceneManager.OpenScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity");
        }
        static void PolishUI()
        {
            var dialogue=UnityEngine.Object.FindAnyObjectByType<DialogueUI>();var panel=Field<GameObject>(dialogue,"dialoguePanel");Rect(panel.GetComponent<RectTransform>(),new Vector2(0,35),new Vector2(1380,330));
            var body=Field<TextMeshProUGUI>(dialogue,"dialogueText");Rect(body.rectTransform,new Vector2(28,-74),new Vector2(1324,190));body.fontSize=26;body.enableAutoSizing=false;
            var next=Field<Button>(dialogue,"continueButton");Rect(next.GetComponent<RectTransform>(),new Vector2(-24,18),new Vector2(200,46));
            var choices=UnityEngine.Object.FindAnyObjectByType<DialogueChoiceUI>();var choicePanel=Field<GameObject>(choices,"panel");Rect(choicePanel.GetComponent<RectTransform>(),new Vector2(0,385),new Vector2(1380,300));
            var result=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>();var end=Field<GameObject>(result,"endPanel");Rect(end.GetComponent<RectTransform>(),Vector2.zero,new Vector2(1150,740));
            var message=Field<TextMeshProUGUI>(result,"endMessage");message.rectTransform.anchorMin=message.rectTransform.anchorMax=message.rectTransform.pivot=new Vector2(.5f,.5f);Rect(message.rectTransform,new Vector2(0,0),new Vector2(1020,430));message.fontSize=27;
            var title=Field<TextMeshProUGUI>(result,"endTitle");title.rectTransform.pivot=new Vector2(.5f,.5f);title.rectTransform.anchorMin=title.rectTransform.anchorMax=new Vector2(.5f,1);Rect(title.rectTransform,new Vector2(0,-55),new Vector2(1020,80));title.alignment=TextAlignmentOptions.Center;
            var nextButton=Field<Button>(result,"nextButton");nextButton.GetComponent<RectTransform>().anchorMin=nextButton.GetComponent<RectTransform>().anchorMax=new Vector2(.5f,0);Rect(nextButton.GetComponent<RectTransform>(),new Vector2(0,75),new Vector2(680,70));
            var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();var pp=Field<GameObject>(pause,"pausePanel");Rect(pp.GetComponent<RectTransform>(),Vector2.zero,new Vector2(850,620));
            var resume=Field<Button>(pause,"resumeButton");var back=Field<Button>(pause,"restartButton");var quit=Field<Button>(pause,"quitButton");
            var buttons=new[]{resume,back,quit};for(int i=0;i<3;i++){var r=buttons[i].GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);Rect(r,new Vector2(0,100-i*90),new Vector2(650,65));}
            back.GetComponentInChildren<TMP_Text>().text="Kembali ke menu · checkpoint terakhir";
            var settings=pp.transform.Find("SettingsAndHelp")?.GetComponent<Button>()??UnityEngine.Object.Instantiate(resume,pp.transform);settings.name="SettingsAndHelp";settings.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(settings.onClick,pause.ShowSettings);settings.GetComponentInChildren<TMP_Text>().text="Pengaturan dan bantuan";Rect(settings.GetComponent<RectTransform>(),new Vector2(0,-170),new Vector2(650,65));
            foreach(var accessibility in UnityEngine.Object.FindObjectsByType<PlayerAccessibility>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                foreach(var key in new[]{"instantText","reducedPressure","assistedQTE"}){var toggle=Field<Toggle>(accessibility,key);if(toggle!=null){toggle.gameObject.SetActive(false);if(toggle.transform.parent.name=="ComfortSettings")toggle.transform.parent.gameObject.SetActive(false);}}
            var quiz=UnityEngine.Object.FindAnyObjectByType<BullyingGame.Quiz.QuizUI>();
            var quizPanel=Field<GameObject>(quiz,"panelRoot");quizPanel.GetComponent<Image>().color=new Color(.035f,.07f,.09f,1);
            var continueQuiz=Field<Button>(quiz,"continueButton");continueQuiz.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,270);
            foreach(var button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                foreach(var text in button.GetComponentsInChildren<TMP_Text>(true))if(text.transform.parent==button.transform){var r=text.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(-20,-8);}
            foreach(var text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(text.text.Contains("Space QTE")||text.text.Contains("tiga perspektif"))text.text="J jurnal · M peta · Esc jeda";
                if(text.name=="QuestHeading")text.text="MISI";
                if(text.text=="Lewati pembukaan")text.text="Lewati adegan";
                if(text.text=="QUEST • PILIH TANGGAPAN")text.text="REFLEKSI • PILIH TANGGAPAN";
                if(text.name=="Hint"&&text.transform.parent==pp.transform)text.text="Checkpoint disimpan setelah tujuan selesai. Keluar saat dialog kembali ke checkpoint terakhir.";
            }
        }
        static void Menu()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/01_MainMenu/01_MainMenu.unity");var menu=UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>();Refs(menu,"chapters",Enumerable.Range(1,5).Select(i=>(UnityEngine.Object)Load<NarrativeChapterData>("Chapter0"+i)).ToArray());
            var newGame=Field<Button>(menu,"newGameButton");var cont=Field<Button>(menu,"continueButton");var settings=Field<Button>(menu,"settingsButton");var quit=Field<Button>(menu,"quitButton");
            var help=settings.transform.parent.Find("HelpButton")?.GetComponent<Button>()??UnityEngine.Object.Instantiate(settings,settings.transform.parent);help.name="HelpButton";help.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(help.onClick,menu.ShowHelp);help.GetComponentInChildren<TMP_Text>().text="Bantuan kontrol";
            var buttons=new[]{newGame,cont,settings,help,quit};for(int i=0;i<buttons.Length;i++){var r=buttons[i].GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);Rect(r,new Vector2(0,170-i*90),new Vector2(650,68));}
            foreach(var t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(t.text.Contains("perspektif")||t.text.Contains("POV")||t.text.Contains("QTE"))t.text="Petualangan Aji · Dukungan aman · 5 chapter, 15 misi";
            var status=Field<TMP_Text>(menu,"statusText");if(status!=null){status.rectTransform.anchorMin=status.rectTransform.anchorMax=new Vector2(.5f,0);Rect(status.rectTransform,new Vector2(0,90),new Vector2(1500,110));status.fontSize=23;}
            foreach(var t in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(t.name=="Description"||t.name=="NewGameNote")t.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        static void Blockout()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Environment/SchoolEnvironment.prefab"));
            EditorSceneManager.SaveScene(scene,"Assets/_Game/Scenes/Levels/env_design/School_PRD_Blockout.unity");
        }
    }
}


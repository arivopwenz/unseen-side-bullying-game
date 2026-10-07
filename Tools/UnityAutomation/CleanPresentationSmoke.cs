using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using TMPro;
using BullyingGame.Core;
using BullyingGame.UI;
using BullyingGame.Dialogue;
using BullyingGame.Cinematics;
using BullyingGame.Quest;
using BullyingGame.Save;
using BullyingGame.Events;

public static class CleanPresentationSmoke
{
 static int checks;
 static StoryChapterSequence Sequence=>UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
 static T F<T>(UnityEngine.Object o,string field) where T:UnityEngine.Object=>new SerializedObject(o).FindProperty(field).objectReferenceValue as T;
 static void Check(bool condition,string message){if(!condition)throw new Exception("FAIL: "+message);checks++;}
 static void RightPanel()
 {
 var hud=UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>();var panel=F<GameObject>(hud,"hudContainer").GetComponent<RectTransform>();var corners=new Vector3[4];panel.GetWorldCorners(corners);foreach(var corner in corners){var point=RectTransformUtility.WorldToScreenPoint(null,corner);Check(point.x>Screen.width*.65f&&point.x<Screen.width&&point.y>0&&point.y<Screen.height,"Quest panel entirely inside right side "+point);}
 }
 static async Task Wait(Func<bool> condition,string message,int seconds=20){double end=EditorApplication.timeSinceStartup+seconds;while(!condition()&&EditorApplication.timeSinceStartup<end)await Task.Delay(25);Check(condition(),message);}
 static async Task Capture(string n){Directory.CreateDirectory("Logs/CleanPresentation");var path=Path.GetFullPath("Logs/CleanPresentation/"+n+".png");if(File.Exists(path))File.Delete(path);ScreenCapture.CaptureScreenshot(path);await Wait(()=>File.Exists(path),"Capture "+n,5);}
 static void Lines(){var original=DialogueManager.Instance.CurrentDialogue;int guard=0;while(DialogueManager.Instance.IsDialogueActive&&DialogueManager.Instance.CurrentDialogue==original&&guard++<80){if(DialogueManager.Instance.IsAwaitingChoice)DialogueManager.Instance.SelectChoice(0);else DialogueManager.Instance.NextLine();}}
 public static string Integrity()
 {
 checks=0;Check(!Application.isPlaying,"Edit Mode");Check(EditorBuildSettings.scenes.Count(s=>s.enabled)==7,"Bootstrap/menu + five levels");
 for(int number=1;number<=5;number++)
 {
 EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Level_0"+number+"/Level0"+number+".unity");Check(new SerializedObject(Sequence).FindProperty("sceneChapterNumber").intValue==number,"One chapter host per scene");
 var current=AssetDatabase.LoadAssetAtPath<NarrativeChapterData>("Assets/_Game/Data/StoryPrototype/Chapter0"+number+".asset");Check(current.SceneName==SceneManager.GetActiveScene().name,"Scene data routing");
 var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();var director=F<PlayableDirector>(opening,"timeline");var timeline=(TimelineAsset)director.playableAsset;var track=timeline.GetOutputTracks().OfType<CinemachineTrack>().Single();Check(track.GetClips().Count()==10,"Establishing / eight character shots / closing");foreach(var clip in track.GetClips()){var camera=((CinemachineShot)clip.asset).VirtualCamera.Resolve(director);Check(camera!=null&&camera.gameObject.scene==SceneManager.GetActiveScene(),"Shot bound inside its own scene");}
 var story=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>();Check(!F<TextMeshProUGUI>(story,"chapterText").transform.parent.gameObject.activeSelf,"Chapter/POV HUD removed");Check(!story.GetComponent<StoryLearningHint>().enabled,"Duplicate instruction disabled");
 var hud=UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>();var rect=F<GameObject>(hud,"hudContainer").GetComponent<RectTransform>();Check(rect.anchorMax==Vector2.one&&rect.pivot==Vector2.one,"Quest right aligned");Check(rect.sizeDelta.x==440,"Compact quest width");
 foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script: "+t.name);Check(true,"No missing script");
 }
 var c1=AssetDatabase.LoadAssetAtPath<NarrativeChapterData>("Assets/_Game/Data/StoryPrototype/Chapter01.asset");Check(c1.Reflections.All(q=>!q.questionText.Contains("QTE")),"Questions concern story, not controls");
 EditorSceneManager.OpenScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity");return "PASS clean scene integrity: "+checks+" checks; five scene hosts, all Timeline references, compact HUD, clean questions and no missing scripts.";
 }
        public static async Task<string> OpeningAndHUD()
 {
 checks=0;Check(Application.isPlaying&&SaveManager.Instance!=null,"Bootstrap Play Mode");Application.runInBackground=true;Directory.CreateDirectory("Logs/CleanPresentation");typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(SaveManager.Instance,Path.GetFullPath("Logs/CleanPresentation/savegame.json"));SaveManager.Instance.ResetProgress();GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level01");
 await Wait(()=>Sequence!=null&&UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>()!=null&&UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().IsRunning,"Timeline opening starts");typeof(DialogueUI).GetField("typingSpeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(UnityEngine.Object.FindAnyObjectByType<DialogueUI>(),0f);
 await Wait(()=>DialogueManager.Instance!=null&&DialogueManager.Instance.IsDialogueActive,"Signal starts introduction");var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();var hud=UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>();var story=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>();var dp=F<GameObject>(UnityEngine.Object.FindAnyObjectByType<DialogueUI>(),"dialoguePanel");var skip=F<UnityEngine.UI.Button>(story,"skipButton");Check(skip.transform.parent==dp.transform,"Skip is in dialogue top right");Check(F<PlayableDirector>(opening,"timeline").playableAsset.name=="TL_Level01_Opening","Real character Timeline asset");
 int count=0;while(DialogueManager.Instance.IsDialogueActive&&count++<30){var line=DialogueManager.Instance.CurrentLine;await Task.Delay(350);Check(!F<GameObject>(hud,"hudContainer").activeInHierarchy,"Quest hidden in introduction");if(new[]{"Aji","Denis","Billy","Ari","Pak Bambang"}.Contains(line.speakerName))await Capture("Intro_"+line.speakerName.Replace(" ",""));DialogueManager.Instance.NextLine();}
 await Wait(()=>!opening.IsRunning&&Sequence.ActiveQuest!=null&&GameStateManager.Instance.CurrentState==GameState.Playing,"Opening restores gameplay");await Task.Delay(800);Check(F<GameObject>(hud,"hudContainer").activeInHierarchy,"Quest visible in gameplay");Check(!F<TextMeshProUGUI>(story,"chapterText").transform.parent.gameObject.activeInHierarchy,"No POV header");Check(!story.GetComponent<StoryLearningHint>().enabled,"No bottom explanatory text");UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>().Teleport(Vector3.zero,Quaternion.identity);await Task.Delay(600);Check(F<RectTransform>(story.GetComponent<MissionNavigationHint>(),"directionArrow").gameObject.activeInHierarchy,"Direction arrow points to first quest from outside arrival radius");RightPanel();await Capture("CleanHUD");return "PASS opening/HUD: "+checks+" checks; actual signal-driven character shots, skip placement, gameplay arrow and cinematic hiding.";
 }
 public static async Task<string> ContinueAndMigration()
 {
 checks=0;Check(Sequence!=null&&Sequence.ChapterNumber==5&&Sequence.IsChapterComplete,"Final isolated result");Sequence.AdvanceChapter();await Wait(()=>UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>()!=null && GameStateManager.Instance.CurrentState==GameState.MainMenu,"Campaign returns menu");var menu=UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>();F<UnityEngine.UI.Button>(menu,"continueButton").onClick.Invoke();await Wait(()=>Sequence!=null&&Sequence.ChapterNumber==5&&Sequence.IsChapterComplete,"Actual Continue loads completed chapter");Check(SceneManager.GetActiveScene().name=="Level05","Continue goes to Level05");
 // Legacy menu saved chapter 2 without chapter 1 prerequisites: scene host redirects safely.
 SaveManager.Instance.ResetProgress();SaveManager.Instance.CurrentData.currentChapter=2;SaveManager.Instance.SaveGame();GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level02");await Wait(()=>Sequence!=null&&SceneManager.GetActiveScene().name=="Level01"&&UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().IsRunning,"Legacy incomplete checkpoint redirects to Level01");Check(Sequence.ChapterNumber==1,"No chapter 1 quest inside Level02");return "PASS Continue/migration: "+checks+" checks; actual Continue scene routing and earliest unfinished legacy chapter redirect.";
 }
 public static async Task<string> GroupFraming()
 {
 checks=0;var activity=UnityEngine.Object.FindAnyObjectByType<NarrativeActivityDirector>();Check(activity!=null&&activity.IsRunning,"Activity active");await Task.Delay(1200);var camera=UnityEngine.Camera.main;var player=GameObject.Find("Player").transform;var actors=new[]{player,GameObject.Find("_Level/BullyGroup/Bully_Leader").transform,GameObject.Find("_Level/BullyGroup/Bully_Friends").transform,GameObject.Find("_Level/BullyGroup/Bully_Friends 2").transform};foreach(var actor in actors){var head=camera.WorldToViewportPoint(actor.position+Vector3.up*1.8f);Check(head.z>0&&head.x>.035f&&head.x<.965f&&head.y>.29f&&head.y<.95f,"Actor head safely framed "+actor.name);}Check(!F<GameObject>(UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>(),"hudContainer").activeInHierarchy,"Activity hides quest");await Capture("GroupFraming");return "PASS group framing: "+checks+" checks / Denis and all three bullies safely in frame.";
 }
 public static string DiagnoseNav()
 {
 var nav=UnityEngine.Object.FindAnyObjectByType<MissionNavigationHint>();var so=new SerializedObject(nav);var chars=so.FindProperty("characters");string value="state="+GameStateManager.Instance.CurrentState+" quest="+Sequence.ActiveQuest.questId+" navEnabled="+nav.enabled+" text="+F<TMP_Text>(nav,"hintText").text+" arrowSelf="+F<RectTransform>(nav,"directionArrow").gameObject.activeSelf+"\n";
 var escort=F<BullyingGame.NPC.EscortCompanion>(nav,"companion");var notebook=F<QuestItem>(nav,"notebook");value+="escort="+(escort!=null && escort.IsEscorting)+" book="+(notebook!=null && notebook.CanInteract())+"\n";var stored=F<StoryChapterSequence>(nav,"sequence");value+="navActive="+nav.isActiveAndEnabled+" storedSame="+(stored==Sequence)+" storedQuest="+(stored!=null && stored.ActiveQuest!=null?stored.ActiveQuest.questId:"none")+" navTime="+Time.unscaledTime+" storedSequence="+(stored!=null)+" player="+F<Transform>(nav,"player").position+"\n";
 for(int i=0;i<chars.arraySize;i++){var npc=chars.GetArrayElementAtIndex(i).objectReferenceValue as BullyingGame.NPC.MissionDialogueNPC;value+=(npc!=null?npc.name+" active="+npc.isActiveAndEnabled+" offers="+npc.OffersObjective(Sequence.ActiveQuest)+" pos="+npc.transform.position:"null")+"\n";}
 return value;
 }
 public static string DiagnoseChapter()
 {
 var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();string s="scene="+SceneManager.GetActiveScene().name+" state="+GameStateManager.Instance.CurrentState+" chapter="+Sequence.ChapterNumber+" complete="+Sequence.IsChapterComplete+" active="+(Sequence.ActiveQuest!=null?Sequence.ActiveQuest.questId:"none")+" opening="+opening.IsRunning+" director="+(CutsceneManager.Instance.CurrentDirector!=null)+" dialogue="+DialogueManager.Instance.IsDialogueActive+"\n";
 foreach(var q in Sequence.CurrentChapter.Missions)s+=q.questId+" state="+QuestManager.Instance.GetQuestState(q)+" applicable="+q.IsApplicable+" prereq="+QuestManager.Instance.PrerequisitesCompleted(q)+"\n";
 var quiz=UnityEngine.Object.FindAnyObjectByType<BullyingGame.Quiz.QuizUI>();var image=F<GameObject>(quiz,"panelRoot").GetComponent<UnityEngine.UI.Image>();s+="cardColor="+image.color+" sprite="+(image.sprite!=null?image.sprite.name:"none")+" alphaGroups="+string.Join(",",image.GetComponentsInParent<CanvasGroup>(true).Select(g=>g.alpha.ToString()));return s;
 }
 public static async Task<string> RefreshNav()
 {
 var nav=UnityEngine.Object.FindAnyObjectByType<MissionNavigationHint>();var text=F<TMP_Text>(nav,"hintText");string result="enabled="+nav.isActiveAndEnabled+" next="+typeof(MissionNavigationHint).GetField("updateAt",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(nav)+" now="+Time.unscaledTime+" hintActive="+text.gameObject.activeInHierarchy;
 typeof(MissionNavigationHint).GetField("updateAt",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(nav,0f);typeof(MissionNavigationHint).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(nav,null);result+=" afterUpdate="+text.text;await Task.Delay(500);result+=" afterFrame="+text.text;await Capture("NavDiagnostic");return result;
 }
}







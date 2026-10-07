using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Animations.Rigging;
using UnityEditor;
using BullyingGame.Core;
using BullyingGame.Cinematics;
using BullyingGame.Quest;
using BullyingGame.NPC;
using BullyingGame.Events;
using BullyingGame.Dialogue;
using BullyingGame.QTE;
using BullyingGame.Quiz;
using BullyingGame.Save;
using BullyingGame.UI;

public static class CampaignSmoke
{
    static int checks;
    static string TestPath => Path.GetFullPath("Logs/AutomationCampaign/savegame.json");
    static void Check(bool result,string text){if(!result)throw new Exception("FAIL: "+text);checks++;}
    static async Task Wait(Func<bool> condition,string text,float seconds=14)
    {double end=EditorApplication.timeSinceStartup+seconds;while(!condition() && EditorApplication.timeSinceStartup<end)await Task.Delay(16);Check(condition(),text);}
    static StoryChapterSequence Sequence => UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
    static GameObject Player => GameObject.Find("Player");
    static void Place(Vector3 position){UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>().Teleport(position,Quaternion.identity);Physics.SyncTransforms();}
    static void EndLines(){int guard=0;while(DialogueManager.Instance.IsDialogueActive && guard++<40)DialogueManager.Instance.NextLine();Check(!DialogueManager.Instance.IsDialogueActive,"Dialogue completed normally");}
    static async Task Talk(MissionDialogueNPC npc)
    {Check(npc!=null && npc.CanInteract(),"NPC ready: "+(npc!=null?npc.name:"missing"));Place(npc.transform.position+Vector3.back*1.7f);npc.Interact(Player);Check(DialogueManager.Instance.IsDialogueActive,"Conversation starts "+npc.name);EndLines();await Task.Delay(40);}
    static bool Objective(QuestData q,string id) => QuestManager.Instance.IsObjectiveCompleted(q,id);
    static async Task Capture(string name)
    {string path=Path.GetFullPath("Logs/AutomationCampaign/"+name+".png");if(File.Exists(path))File.Delete(path);ScreenCapture.CaptureScreenshot(path);await Wait(()=>File.Exists(path),"Screenshot captured "+name,5);}
    public static string SceneIntegrity()
    {
        Check(!Application.isPlaying,"Edit Mode integrity");var sequence=Sequence;var so=new SerializedObject(sequence);var chapters=so.FindProperty("chapters");Check(chapters.arraySize==5,"Five authored chapters");
        var ids=new System.Collections.Generic.HashSet<string>();QuestData prior=null;int total=0;
        for(int i=0;i<5;i++)
        {var data=(NarrativeChapterData)chapters.GetArrayElementAtIndex(i).objectReferenceValue;Check(data!=null && data.Missions.Length>=4,"Each chapter has multiple quests");Check(data.OpeningDialogue!=null && data.OpeningDialogue.lines.Length>=7,"Each chapter has substantial opening dialogue");Check(data.Reflections.Length==2,"Two reflections per chapter");
            foreach(var quest in data.Missions){Check(QuestManager.IsValid(quest),"Valid objective data "+quest.name);Check(ids.Add(quest.questId),"Unique quest identity "+quest.questId);Check(prior==null ? quest.prerequisites.Length==0 : quest.prerequisites.Length==1 && quest.prerequisites[0]==prior,"Ordered prerequisites "+quest.name);prior=quest;total++;}}
        Check(total==22,"22 authored missions");Check(EditorBuildSettings.scenes.Length>=3 && EditorBuildSettings.scenes[0].path.Contains("00_Bootstrap"),"Bootstrap build index zero");
        foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"No missing script "+t.name);
        var rigs=UnityEngine.Object.FindObjectsByType<RigBuilder>(FindObjectsInactive.Include);Check(rigs.Length==8,"Eight authored character rigs");foreach(var rig in rigs){var a=rig.GetComponent<Animator>();Check(a!=null && a.avatar!=null && a.avatar.isValid && a.runtimeAnimatorController!=null,"Valid rig/avatar/controller "+rig.transform.parent.name);}
        return "PASS scene integrity: "+checks+" checks, 5 outdoor chapters / 22 missions / 10 reflections / 8 character rigs.";
    }
    public static async Task<string> NotebookFailure()
    {
        Check(Application.isPlaying,"Play Mode required");Application.runInBackground=true;
        Check(SaveManager.Instance!=null,"Save manager exists");typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(SaveManager.Instance,TestPath);SaveManager.Instance.ResetProgress();
        var sequence=Sequence;var known=new SerializedObject(sequence).FindProperty("chapters");var quests=Enumerable.Range(0,known.arraySize).SelectMany(i=>((NarrativeChapterData)known.GetArrayElementAtIndex(i).objectReferenceValue).Missions).ToArray();
        var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();if(opening.IsRunning)opening.Skip();DialogueManager.Instance.EndDialogue();QuestManager.Instance.RestoreProgress(new QuestProgressRecord[0],quests);BullyingEventManager.Instance.RestoreHandledEvents(new string[0]);
        typeof(StoryChapterSequence).GetField("chapterIndex",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(sequence,0);GameStateManager.Instance.SetState(GameState.Playing);UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>().Apply(POVType.Victim);typeof(StoryChapterSequence).GetMethod("EnterChapter",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sequence,new object[]{true});
        await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Timeline signal starts opening dialogue");
        var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();var line=DialogueManager.Instance.CurrentLine;pause.PauseGame();Check(GameStateManager.Instance.CurrentState==GameState.Paused,"Pause opening dialogue");UnityEngine.Object.FindAnyObjectByType<DialogueUI>().SkipOrNext();await Task.Delay(130);Check(DialogueManager.Instance.CurrentLine==line,"Dialogue frozen during pause");pause.ResumeGame();Check(GameStateManager.Instance.CurrentState==GameState.Cinematic,"Resume prior cinematic state");await Capture("Opening");EndLines();
        await Wait(()=>!opening.IsRunning && sequence.ActiveQuest!=null,"Opening finishes and first quest starts",16);
        Check(Player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Movement unlocked after normal opening");
        var book=UnityEngine.Object.FindAnyObjectByType<QuestItem>();Check(!book.CanInteract(),"Book gated before its quest");var first=sequence.ActiveQuest;var dimas=GameObject.Find("_Level/StoryCharacters/Dimas").GetComponent<MissionDialogueNPC>();
        dimas.Interact(Player);DialogueManager.Instance.EndDialogue();Check(!Objective(first,"listen_friend"),"Cancelled conversation does not complete objective");await Talk(dimas);await Wait(()=>sequence.ActiveQuest!=first,"First mission advances");var bookQuest=sequence.ActiveQuest;
        Check(!book.CanInteract(),"Book gated before accepting request");await Talk(GameObject.Find("_Level/BullyGroup/Bully_Leader").GetComponent<MissionDialogueNPC>());Check(Objective(bookQuest,"accept_request") && book.CanInteract(),"Request unlocks notebook");
        var origin=book.transform.position;Place(origin+Vector3.back);book.Interact(Player);await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Actual bully approach reaches confrontation",18);await Task.Delay(600);
        foreach(var npc in UnityEngine.Object.FindObjectsByType<BullyNPC>())foreach(float height in new[]{.15f,1.85f}){var p=UnityEngine.Camera.main.WorldToViewportPoint(npc.transform.position+Vector3.up*height);Check(p.z>0 && p.x>.02f && p.x<.98f && p.y>.02f && p.y<.98f,"Notebook camera contains "+npc.name+" "+p);}
        await Capture("NotebookConfrontation");EndLines();await Wait(()=>QTEManager.Instance.IsQTEActive,"Real Timeline QTE starts");await Capture("QTE");var time=QTEManager.Instance.RemainingTime;pause.PauseGame();await Task.Delay(180);Check(Mathf.Abs(QTEManager.Instance.RemainingTime-time)<.05f,"QTE timer stops on pause");pause.ResumeGame();Check(GameStateManager.Instance.CurrentState==GameState.QTE,"Resume QTE state");
        await Wait(()=>book!=null && book.CanInteract(),"Failure returns to gameplay",25);
        Check(book.State==QuestItemState.Rehidden,"Book state is rehidden");Check(Vector3.Distance(origin,book.transform.position)>2,"Book randomly moved away from origin");Check(!Objective(bookQuest,"find_notebook"),"Failure leaves find objective pending");Check(BullyingEventManager.Instance.CaptureHandledEvents().Length==1,"Encounter marked handled exactly once");
        var checkpoint=UnityEngine.Object.FindAnyObjectByType<SceneCheckpoint>();Check(checkpoint.CaptureNow(),"Failure checkpoint saved to isolated test slot");var saved=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(TestPath));var record=saved.items.First(i=>i.itemId==book.CaptureCheckpoint().itemId);var rehidden=book.transform.position;book.transform.position=origin;book.RestoreCheckpoint(record);Check(Vector3.Distance(book.transform.position,rehidden)<.01f,"Checkpoint restores same hide anchor");
        Place(rehidden+Vector3.back*.8f);await Task.Delay(120);Check(ReferenceEquals(Player.GetComponent<BullyingGame.Interaction.InteractionDetector>().CurrentInteractable,book),"Actual detector finds relocated book");book.Interact(Player);
        Check(Objective(bookQuest,"find_notebook") && QuestManager.Instance.GetQuestState(bookQuest)==QuestState.Active,"Recollect secures book, turn-in still pending");Check(CutsceneManager.Instance.CurrentDirector==null && GameStateManager.Instance.CurrentState==GameState.Playing,"Recollect does not replay cinematic");await Talk(GameObject.Find("_Level/BullyGroup/Bully_Leader").GetComponent<MissionDialogueNPC>());await Wait(()=>sequence.ActiveQuest!=bookQuest,"Turn-in advances quest flow");
        foreach(var rig in UnityEngine.Object.FindObjectsByType<RigBuilder>())Check(rig.graph.IsValid(),"Runtime rig graph valid "+rig.transform.parent.name);
        await Capture("Gameplay");return "PASS notebook integration: "+checks+" checks; normal opening, cancellation, camera, QTE/pause, random rehide, checkpoint, real detector, recollect and turn-in.";
    }
    public static async Task<string> CompleteCampaign()
    {
        Check(Application.isPlaying && SaveManager.Instance!=null,"Play Mode setup ready");Check((string)typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(SaveManager.Instance)==TestPath,"Isolated test save retained");
        var sequence=Sequence;var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();int iterations=0,completedChapters=0;
        while(completedChapters<5 && iterations++<90)
        {
            if(opening.IsRunning){opening.Skip();await Task.Delay(80);Check(!opening.IsRunning && Player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Skipped opening unlocks controls");}
            if(QuizManager.Instance.IsQuizActive)
            {var quiz=QuizManager.Instance;int score=SaveManager.Instance.CurrentData.empathyScore;quiz.SubmitAnswer(-1);Check(!quiz.IsAnswered,"Invalid quiz answer ignored");quiz.SubmitAnswer(quiz.CurrentQuestion.correctOptionIndex);quiz.SubmitAnswer(quiz.CurrentQuestion.correctOptionIndex);Check(SaveManager.Instance.CurrentData.empathyScore==score+10,"Quiz awards score once");await Capture("Reflection_"+sequence.ChapterNumber);quiz.CloseQuiz();await Task.Delay(60);continue;}
            if(sequence.IsChapterComplete)
            {completedChapters++;Check(GameStateManager.Instance.CurrentState==GameState.Result,"Chapter result modal");Check(!Player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Result locks movement");await Capture("ChapterEnd_"+sequence.ChapterNumber);if(sequence.ChapterNumber==5)break;int before=sequence.ChapterNumber;sequence.AdvanceChapter();await Task.Delay(80);Check(sequence.ChapterNumber==before+1,"Chapter advances in order");Check(POVManager.Instance.CurrentPOV==sequence.CurrentChapter.Perspective,"One Player uses correct POV");continue;}
            var escort=UnityEngine.Object.FindAnyObjectByType<EscortCompanion>();if(escort.IsEscorting)
            {Place(escort.Destination.position);await Wait(()=>!escort.IsEscorting,"Actual NavMesh escort reaches safe zone",14);continue;}
            var quest=sequence.ActiveQuest;if(quest==null){await Task.Delay(80);continue;}
            var npc=UnityEngine.Object.FindObjectsByType<MissionDialogueNPC>().FirstOrDefault(n=>n.OffersObjective(quest));Check(npc!=null,"A reachable NPC handles mission "+quest.questId);await Talk(npc);await Task.Delay(50);
        }
        Check(completedChapters==5 && sequence.ChapterNumber==5 && sequence.IsChapterComplete,"Five chapters completed through actual NPC/quiz routes");
        Check(QuestManager.Instance.CaptureProgress().Count(r=>r.state==QuestState.Completed)==22,"22 missions complete");Check(SaveManager.Instance.CurrentData.empathyScore==100,"10 reflections award exactly 100");Check(SaveManager.Instance.CurrentData.campaignCompleted,"Campaign completion saved");Check(UnityEngine.Object.FindAnyObjectByType<SceneCheckpoint>().CaptureNow(),"Final checkpoint write");
        var saved=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(TestPath));Check(saved.currentChapter==5 && saved.campaignCompleted,"Chapter five round trip");Check(saved.quests.Length==22 && saved.quests.All(q=>q.objectiveIds!=null),"Objective IDs persist");
        SaveManager.Instance.CurrentData.empathyScore=90;Check(SaveManager.Instance.TrySaveGame(),"Atomic save overwrite");File.WriteAllText(TestPath,"broken primary save");SaveManager.Instance.LoadGame();Check(SaveManager.Instance.CurrentData.empathyScore==100 && SaveManager.Instance.CurrentData.currentChapter==5,"Backup restores complete campaign after damaged primary");Check(SaveManager.Instance.TrySaveGame(),"Restore valid test primary");
        return "PASS campaign integration: "+checks+" checks; 5 chapters, actual conversations/escort/POV transitions, 22 missions, 10 reflections, modal controls and save backup round trip.";
    }
}

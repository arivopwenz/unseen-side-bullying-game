using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.Quest;
using BullyingGame.NPC;
using BullyingGame.Cinematics;
using BullyingGame.QTE;
using BullyingGame.Quiz;
using BullyingGame.Events;
using BullyingGame.Save;
using BullyingGame.UI;

public static class Chapter12Smoke
{
    static int checks;
    const string Folder="Assets/_Game/Data/StoryPrototype/";
    static string PathSave=>Path.GetFullPath("Logs/AutomationChapter12/savegame.json");
    static StoryChapterSequence Sequence=>UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
    static GameObject Player=>GameObject.Find("Player");
    static void Check(bool result,string message){if(!result)throw new Exception("FAIL: "+message);checks++;}
    static async Task Wait(Func<bool> condition,string message,float seconds=15){double end=EditorApplication.timeSinceStartup+seconds;while(!condition()&&EditorApplication.timeSinceStartup<end)await Task.Delay(20);Check(condition(),message);}
    static void Place(Vector3 position)=>UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>().Teleport(position,Quaternion.identity);
    static void Lines(int choice=0){var original=DialogueManager.Instance.CurrentDialogue;int guard=0;while(DialogueManager.Instance.IsDialogueActive&&DialogueManager.Instance.CurrentDialogue==original&&guard++<70){if(DialogueManager.Instance.IsAwaitingChoice)Check(DialogueManager.Instance.SelectChoice(choice),"Aji selects valid response");else DialogueManager.Instance.NextLine();}Check(!DialogueManager.Instance.IsDialogueActive||DialogueManager.Instance.CurrentDialogue!=original,"Original dialogue completed normally");}
    static async Task Capture(string name){string path=Path.GetFullPath("Logs/AutomationChapter12/"+name+".png");if(File.Exists(path))File.Delete(path);ScreenCapture.CaptureScreenshot(path);await Wait(()=>File.Exists(path),"Capture "+name,5);}
    static void GroupVisible()
    {
        var camera=UnityEngine.Camera.main;
        foreach(var actor in new[]{Player.transform,GameObject.Find("_Level/BullyGroup/Bully_Leader").transform,GameObject.Find("_Level/BullyGroup/Bully_Friends").transform,GameObject.Find("_Level/BullyGroup/Bully_Friends 2").transform})
        {
            var head=camera.WorldToViewportPoint(actor.position+Vector3.up*1.9f);var feet=camera.WorldToViewportPoint(actor.position+Vector3.up*.1f);
            Check(head.z>0&&head.x>.025f&&head.x<.975f&&head.y<.97f&&feet.y>.27f,"Full actor above dialogue area "+actor.name+" / head="+head+" feet="+feet);
        }
        var hud=UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>();Check(!((GameObject)new SerializedObject(hud).FindProperty("hudContainer").objectReferenceValue).activeInHierarchy,"Quest hidden during activity");
    }
    static async Task Talk(MissionDialogueNPC npc,int choice=0)
    {Check(npc!=null&&npc.CanInteract(),"Interactable "+(npc!=null?npc.name:"missing"));Place(npc.transform.position+Vector3.back*1.5f);npc.Interact(Player);Check(DialogueManager.Instance.IsDialogueActive,"NPC dialogue starts");Lines(choice);await Task.Delay(70);}
    static async Task Fresh(int chapter,int priorChoice=0)
    {
        Check(Application.isPlaying&&SaveManager.Instance!=null,"Play Mode with SaveManager");Application.runInBackground=true;
        Directory.CreateDirectory(Path.GetDirectoryName(PathSave));typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(SaveManager.Instance,PathSave);
        SaveManager.Instance.ResetProgress();
        if(chapter==2)
        {
            SaveManager.Instance.RecordNarrativeChoice("aji_first_response",priorChoice);
            var first=AssetDatabase.LoadAssetAtPath<NarrativeChapterData>(Folder+"Chapter01.asset");
            SaveManager.Instance.CurrentData.quests=first.Missions.Where(q=>q.IsApplicable).Select(q=>new QuestProgressRecord{questId=q.questId,state=QuestState.Completed,objectiveIds=q.objectives.Select(o=>o.objectiveId).ToArray(),amounts=q.objectives.Select(o=>o.requiredAmount).ToArray()}).ToArray();
            SaveManager.Instance.CurrentData.currentChapter=2;SaveManager.Instance.SaveGame();
        }
        var previousSequence=Sequence;GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level0"+chapter);await Wait(()=>Sequence!=null&&Sequence!=previousSequence&&UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().IsRunning,"Fresh chapter opens");
        typeof(DialogueUI).GetField("typingSpeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(UnityEngine.Object.FindAnyObjectByType<DialogueUI>(),0f);
        await Wait(()=>DialogueManager.Instance!=null&&DialogueManager.Instance.IsDialogueActive,"Actual Timeline opening signal");
        Check(Sequence.ChapterNumber==chapter,"Correct chapter");Check(POVManager.Instance.CurrentPOV==(chapter==1?POVType.Protagonist:POVType.Victim),"Correct Aji/Denis POV");
        Lines();UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().Skip();await Wait(()=>Sequence.ActiveQuest!=null,"First mission ready");
    }
    public static string Integrity()
    {
        checks=0;Check(!Application.isPlaying&&Sequence!=null,"Edit Mode Level01");
        var chapters=new SerializedObject(Sequence).FindProperty("chapters");Check(chapters.arraySize==5,"Five chapters preserved");var ids=new System.Collections.Generic.HashSet<string>();int count=0;
        for(int i=0;i<5;i++){var chapter=chapters.GetArrayElementAtIndex(i).objectReferenceValue as NarrativeChapterData;Check(chapter!=null,"Chapter data");Check(chapter.AllowsDialogueChoices==(i==0),"Only Aji chapter has choices");foreach(var quest in chapter.Missions){Check(QuestManager.IsValid(quest),"Valid quest "+quest.name);Check(ids.Add(quest.questId),"Unique ID "+quest.questId);count++;}}
        Check(count==26,"26 definitions including 3 mutually exclusive routes");Check(AssetDatabase.LoadAssetAtPath<NarrativeChapterData>(Folder+"Chapter01.asset").PlayerName=="Aji","Aji chapter 1");Check(AssetDatabase.LoadAssetAtPath<NarrativeChapterData>(Folder+"Chapter02.asset").PlayerName=="Denis","Denis chapter 2");
        var ground=GameObject.Find("_Level/Environtment/Ground").GetComponent<Renderer>().bounds;Check(Mathf.Abs(ground.size.x-64)<.1f&&Mathf.Abs(ground.size.z-56)<.1f,"Ground 64 x 56 m");
        var path=new NavMeshPath();foreach(var name in new[]{"_Level/StoryCharacters/Denis","_Level/StoryCharacters/Ari","_Level/StoryCharacters/KantinVendor"}){var target=GameObject.Find(name).transform.position;Check(NavMesh.CalculatePath(Vector3.zero,target,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"Reachable "+name);}
        Check(NavMesh.CalculatePath(Vector3.zero,new Vector3(-20,0,15),NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"Classroom reachable through doorway");
        foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"No missing script "+t.name);
        return "PASS Chapter12 integrity: "+checks+" checks / 26 definitions, 24 playable quests per route, accessible classroom/canteen and no missing scripts.";
    }
    static async Task<string> Route(int choice)
    {
        checks=0;await Fresh(1);var npc=GameObject.Find("_Level/StoryCharacters/Denis").GetComponent<MissionDialogueNPC>();
        npc.Interact(Player);while(!DialogueManager.Instance.IsAwaitingChoice)DialogueManager.Instance.NextLine();Check(SaveManager.Instance.GetNarrativeChoice("aji_first_response")==-1,"Unfinished choice absent");Check(DialogueManager.Instance.SelectChoice(choice),"Choice accepted");DialogueManager.Instance.EndDialogue();Check(!QuestManager.Instance.IsObjectiveCompleted(Sequence.ActiveQuest,"listen_friend"),"Cancelled response gives no objective");Check(SaveManager.Instance.GetNarrativeChoice("aji_first_response")==-1,"Cancelled response gives no checkpoint choice");
        npc.Interact(Player);while(!DialogueManager.Instance.IsAwaitingChoice)DialogueManager.Instance.NextLine();await Task.Delay(1200);await Capture("AjiChoices_"+choice);Check(!DialogueManager.Instance.SelectChoice(4),"Out of range response rejected");Lines(choice);await Wait(()=>Sequence.ActiveQuest.questId.StartsWith("ch01_route_"),"Branch quest opens");
        string id=new[]{"ch01_route_support","ch01_route_provoke","ch01_route_ignore"}[choice];Check(Sequence.ActiveQuest.questId==id,"Selected route only");Check(SaveManager.Instance.GetNarrativeChoice("aji_first_response")==choice,"Completed choice saved");
        for(int i=0;i<3&&Sequence.ActiveQuest.questId==id;i++){var target=UnityEngine.Object.FindObjectsByType<MissionDialogueNPC>().First(n=>n.OffersObjective(Sequence.ActiveQuest));await Talk(target);}
        await Wait(()=>Sequence.ActiveQuest.questId=="quest_level01_lost_notebook","Routes converge on Denis book");Check(!UnityEngine.Object.FindAnyObjectByType<QuestItem>().CanInteract(),"Notebook locked before Denis request");await Talk(npc);Check(UnityEngine.Object.FindAnyObjectByType<QuestItem>().CanInteract(),"Denis request unlocks notebook");
        Check(UnityEngine.Object.FindAnyObjectByType<SceneCheckpoint>().CaptureNow(),"Isolated route checkpoint");SaveManager.Instance.LoadGame();Check(SaveManager.Instance.GetNarrativeChoice("aji_first_response")==choice,"Route choice disk roundtrip");
        var checkpointQuest=Sequence.ActiveQuest.questId;var previousSequence=Sequence;GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level01");await Wait(()=>Sequence!=null&&Sequence!=previousSequence&&Sequence.ActiveQuest!=null,"Route reload");Check(Sequence.ActiveQuest.questId==checkpointQuest,"Reload resumes book without replaying route");Check(!UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().IsRunning,"Checkpoint skips opening");
        return "PASS Aji route "+choice+": "+checks+" checks, cancellation, concrete branch, Denis request, save/load.";
    }
    public static Task<string> SupportRoute()=>Route(0);
    public static Task<string> ProvokeRoute()=>Route(1);
    public static Task<string> IgnoreRoute()=>Route(2);
    public static async Task<string> FinishChapterOne()
    {
        checks=0;Check(Sequence!=null&&Sequence.ChapterNumber==1&&Sequence.ActiveQuest.questId=="quest_level01_lost_notebook","Continue from route test");
        var book=UnityEngine.Object.FindAnyObjectByType<QuestItem>();var quest=Sequence.ActiveQuest;var origin=book.transform.position;Place(origin+Vector3.back);book.Interact(Player);await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Notebook gang approaches",18);Lines();await Wait(()=>QTEManager.Instance.IsQTEActive,"Notebook Timeline QTE");await Wait(()=>book.CanInteract(),"Timeout rehide returns gameplay",25);Check(book.State==QuestItemState.Rehidden&&Vector3.Distance(origin,book.transform.position)>2,"Random rehide different location");Check(!QuestManager.Instance.IsObjectiveCompleted(quest,"find_notebook"),"Failure keeps objective pending");Place(book.transform.position+Vector3.back*.5f);book.Interact(Player);Check(CutsceneManager.Instance.CurrentDirector==null,"Recollect does not replay");await Talk(GameObject.Find("_Level/StoryCharacters/Denis").GetComponent<MissionDialogueNPC>());await Wait(()=>Sequence.ActiveQuest!=quest,"Return to Denis advances");
        int guard=0;while(!Sequence.IsChapterComplete&&guard++<20){var escort=UnityEngine.Object.FindAnyObjectByType<EscortCompanion>();if(escort.IsEscorting){Place(escort.Destination.position);await Wait(()=>!escort.IsEscorting,"Actual escort reaches larger campus safe zone",18);continue;}if(QuizManager.Instance.IsQuizActive){var quizUI=UnityEngine.Object.FindAnyObjectByType<QuizUI>();var index=QuizManager.Instance.CurrentQuestion.correctOptionIndex;await Capture("QuestQuestion_"+index);((Button[])typeof(QuizUI).GetField("optionButtons",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(quizUI))[index].onClick.Invoke();await Capture("QuestFeedback_"+index);((Button)typeof(QuizUI).GetField("continueButton",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(quizUI)).onClick.Invoke();await Task.Delay(60);continue;}var npc=UnityEngine.Object.FindObjectsByType<MissionDialogueNPC>().FirstOrDefault(n=>n.OffersObjective(Sequence.ActiveQuest));if(npc!=null)await Talk(npc);else await Task.Delay(70);}
        Check(Sequence.IsChapterComplete,"Chapter 1 complete through actual missions");await Capture("Chapter1End");Sequence.AdvanceChapter();await Wait(()=>Sequence!=null&&Sequence.ChapterNumber==2&&UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().IsRunning,"Actual transition to Denis");Check(SceneManager.GetActiveScene().name=="Level02","Chapter transition loads separate scene");Check(POVManager.Instance.CurrentPOV==POVType.Victim,"Denis replaces Aji protagonist");return "PASS Chapter1 end: "+checks+" checks, random rehide, Denis turn-in, escort, Ari, followup and scene transition.";
    }
    static async Task<string> Denis(bool correct,bool effort)
    {
        checks=0;await Fresh(2);Check(!Sequence.CurrentChapter.AllowsDialogueChoices,"No Aji choices for victim");var director=UnityEngine.Object.FindAnyObjectByType<NarrativeActivityDirector>();var teacher=GameObject.Find("_Level/PakBambang").GetComponent<MissionDialogueNPC>();await Talk(teacher);await Wait(()=>director.IsMathAwaitingAnswer,"Untimed arithmetic starts");Check(GameStateManager.Instance.CurrentState==GameState.Quiz,"Math uses modal state");Check(!QuizManager.Instance.IsQuizActive,"Math separate from reflections");Check(!Player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Math locks movement");await Task.Delay(1000);await Capture("Math_"+correct);
        var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();pause.PauseGame();director.SubmitAnswer(0);Check(director.IsMathAwaitingAnswer,"Paused answer rejected");pause.ResumeGame();
        var buttons=(Button[])typeof(NarrativeActivityDirector).GetField("answerButtons",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(director);buttons[correct?0:1].onClick.Invoke();Check(DialogueManager.Instance.IsDialogueActive,"Math aftermath starts");Check(DialogueManager.Instance.CurrentDialogue.name==(correct?"DenisPressure_MathCorrect":"DenisPressure_MathWrong"),"Both math results lead to bullying");Check(!DialogueManager.Instance.IsAwaitingChoice,"No moral choice for Denis");await Task.Delay(900);GroupVisible();await Capture("ClassroomBullying_"+correct);Lines();await Wait(()=>Sequence.ActiveQuest.questId=="ch02_lunch","Math proceeds to lunch");Check(Player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Math returns controls");Check(SaveManager.Instance.CurrentData.empathyScore==0,"Math never awards reflection points");
        var vendor=GameObject.Find("_Level/StoryCharacters/KantinVendor").GetComponent<MissionDialogueNPC>();await Talk(vendor);Check(DialogueManager.Instance.CurrentDialogue.name=="DenisPressure_FoodConfrontation","Group confrontation follows canteen conversation");await Task.Delay(900);GroupVisible();await Capture("FoodConfrontation");Lines();await Wait(()=>QTEManager.Instance.IsQTEActive,"Coercion QTE uses existing input system");await Capture("FoodQTE_"+effort);
        var remaining=QTEManager.Instance.RemainingTime;pause.PauseGame();await Task.Delay(160);Check(Mathf.Abs(remaining-QTEManager.Instance.RemainingTime)<.05f,"Pressure QTE pause");pause.ResumeGame();
        if(effort){var gamepad=InputSystem.AddDevice<Gamepad>();try{for(int i=0;i<8&&QTEManager.Instance.IsQTEActive;i++){InputSystem.QueueStateEvent(gamepad,new GamepadState().WithButton(GamepadButton.South));await Task.Delay(65);InputSystem.QueueStateEvent(gamepad,new GamepadState());await Task.Delay(65);}}finally{InputSystem.RemoveDevice(gamepad);}}
        await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Food aftermath",8);Check(DialogueManager.Instance.CurrentDialogue.name==(effort?"DenisPressure_FoodEffort":"DenisPressure_FoodTimeout"),"Effort result changes response, food stolen in both");await Task.Delay(250);await Capture("FoodAftermath_"+effort);Lines();await Wait(()=>Sequence.ActiveQuest.questId=="ch02_pressure","Coercion completes without softlock");Check(Player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Food event returns movement");await Talk(GameObject.Find("_Level/BullyGroup/Bully_Leader").GetComponent<MissionDialogueNPC>());await Wait(()=>Sequence.ActiveQuest.questId=="ch02_witness","Threat advances to witness");await Talk(GameObject.Find("_Level/StoryCharacters/Ari").GetComponent<MissionDialogueNPC>());
        await Wait(()=>QuizManager.Instance.IsQuizActive,"Actual educational reflection opens");QuizManager.Instance.SubmitAnswer(QuizManager.Instance.CurrentQuestion.correctOptionIndex);QuizManager.Instance.CloseQuiz();await Wait(()=>QuizManager.Instance.IsQuizActive,"Second reflection opens");QuizManager.Instance.SubmitAnswer(QuizManager.Instance.CurrentQuestion.correctOptionIndex);QuizManager.Instance.CloseQuiz();await Wait(()=>Sequence.IsChapterComplete,"Denis chapter completes");Check(SaveManager.Instance.CurrentData.empathyScore==20,"Only two reflections award points");Check(SaveManager.Instance.CurrentData.narrativeChoices.Length==1,"Victim never records Aji decisions");await Capture("Chapter2End_"+effort);Check(UnityEngine.Object.FindAnyObjectByType<SceneCheckpoint>().CaptureNow(),"Chapter2 checkpoint");return "PASS Denis math="+correct+" effort="+effort+": "+checks+" checks, real UI / gamepad taps or timeout, both outcomes, threat, Ari, reflection and controls.";
    }
    public static Task<string> DenisCorrectEffort()=>Denis(true,true);
    public static Task<string> DenisWrongTimeout()=>Denis(false,false);
    public static async Task<string> ActivityGuards()
    {
        checks=0;await Fresh(2);var activity=UnityEngine.Object.FindAnyObjectByType<NarrativeActivityDirector>();var teacher=GameObject.Find("_Level/PakBambang").GetComponent<MissionDialogueNPC>();var quest=Sequence.ActiveQuest;
        await Talk(teacher);Check(activity.IsMathAwaitingAnswer,"Math ready to cancel");var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();pause.PauseGame();activity.Cancel();Check(!activity.IsRunning&&GameStateManager.Instance.CurrentState==GameState.Playing,"Cancel paused activity returns gameplay");Check(Player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Cancelled math restores movement");Check(!QuestManager.Instance.IsObjectiveCompleted(quest,"solve"),"Cancelled math preserves objective");
        await Talk(teacher);activity.SubmitAnswer(0);DialogueManager.Instance.EndDialogue();Check(!activity.IsRunning&&!QuestManager.Instance.IsObjectiveCompleted(quest,"solve"),"Cancelled aftermath does not complete math");
        await Talk(teacher);activity.SubmitAnswer(0);Lines();await Wait(()=>Sequence.ActiveQuest.questId=="ch02_lunch","Math retries successfully");
        var lunchQuest=Sequence.ActiveQuest;var canteen=GameObject.Find("_Level/StoryCharacters/KantinVendor").GetComponent<MissionDialogueNPC>();await Talk(canteen);activity.Cancel();Check(GameStateManager.Instance.CurrentState==GameState.Playing && !QuestManager.Instance.IsObjectiveCompleted(lunchQuest,"encounter"),"Cancel group dialogue preserves objective and restores gameplay");await Talk(canteen);Lines();await Wait(()=>QTEManager.Instance.IsQTEActive,"Retry reaches effort QTE");activity.Cancel();Check(!activity.IsRunning && !QTEManager.Instance.IsQTEActive && !QuestManager.Instance.IsObjectiveCompleted(lunchQuest,"encounter"),"Cancel QTE gives no completion and leaves retry available");int assisted=PlayerPrefs.GetInt("Story.AssistedQTE",0),reduced=PlayerPrefs.GetInt("Story.ReducedPressure",0);
        try
        {
            PlayerPrefs.SetInt("Story.AssistedQTE",1);PlayerPrefs.SetInt("Story.ReducedPressure",1);
            await Talk(GameObject.Find("_Level/StoryCharacters/KantinVendor").GetComponent<MissionDialogueNPC>());
            Check(DialogueManager.Instance.CurrentDialogue.name=="DenisPressure_FoodConfrontation","Assistance keeps the group confrontation");Lines();
            Check(!QTEManager.Instance.IsQTEActive&&DialogueManager.Instance.CurrentDialogue.name=="DenisPressure_FoodEffort","Assistance skips QTE but keeps food aftermath");
            await Task.Delay(1200);var pressure=UnityEngine.Object.FindAnyObjectByType<NarrativePressureUI>();var image=(Image)typeof(NarrativePressureUI).GetField("vignette",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pressure);Check(image.color.a<.02f,"Reduced effects removes pressure overlay");var heartbeat=(AudioSource)typeof(NarrativePressureUI).GetField("heartbeat",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pressure);Check(!heartbeat.isPlaying,"Reduced effects stops heartbeat");
            Lines();await Wait(()=>Sequence.ActiveQuest.questId=="ch02_pressure","Assisted encounter progresses");
        }
        finally {PlayerPrefs.SetInt("Story.AssistedQTE",assisted);PlayerPrefs.SetInt("Story.ReducedPressure",reduced);PlayerPrefs.Save();}
        pause.PauseGame();await Task.Delay(600);await Capture("PauseComfort");pause.ResumeGame();
        return "PASS activity guards: "+checks+" checks, pause/cancel/retry, no premature objectives, assisted QTE, reduced visual/audio effects and preferences restored.";
    }
    public static async Task<string> RemainingCampaign()
    {
        checks=0;Check(Sequence!=null&&Sequence.ChapterNumber==2&&Sequence.IsChapterComplete,"Start at completed Denis chapter");Sequence.AdvanceChapter();int chapters=0,guard=0;
        while(chapters<3&&guard++<60)
        {
            if(Sequence==null || GameStateManager.Instance.CurrentState==GameState.Loading){await Task.Delay(80);continue;}var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();if(opening!=null && opening.IsRunning){opening.Skip();await Task.Delay(100);continue;}
            if(QuizManager.Instance.IsQuizActive){QuizManager.Instance.SubmitAnswer(QuizManager.Instance.CurrentQuestion.correctOptionIndex);QuizManager.Instance.CloseQuiz();await Task.Delay(70);continue;}
            if(Sequence.IsChapterComplete){chapters++;if(Sequence.ChapterNumber==5)break;Sequence.AdvanceChapter();await Task.Delay(90);continue;}
            var quest=Sequence.ActiveQuest;if(quest==null){await Task.Delay(80);continue;}
            if(quest.objectives.Any(o=>o.objectiveId=="reflect")){await Wait(()=>QuizManager.Instance.IsQuizActive||Sequence.IsChapterComplete,"Pending reflection opens");continue;}
            var npc=UnityEngine.Object.FindObjectsByType<MissionDialogueNPC>().FirstOrDefault(n=>n.OffersObjective(quest));Check(npc!=null,"Later chapter objective has active NPC "+quest.questId);await Talk(npc);
        }
        Check(chapters==3&&Sequence.ChapterNumber==5&&Sequence.IsChapterComplete,"Remaining chapters still complete");Check(SaveManager.Instance.CurrentData.campaignCompleted,"Campaign completed flag");Check(UnityEngine.Object.FindAnyObjectByType<SceneCheckpoint>().CaptureNow(),"Final isolated checkpoint");
        SaveManager.Instance.LoadGame();Check(SaveManager.Instance.GetNarrativeChoice("aji_first_response")==0,"Choice survives remaining campaign");
        var previousFinalSequence=Sequence;GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level05");await Wait(()=>Sequence!=null&&Sequence!=previousFinalSequence&&Sequence.IsChapterComplete,"Completed campaign reload");Check(Sequence.ChapterNumber==5,"Reload stays at chapter 5");Check(SceneManager.GetActiveScene().name=="Level05","Final result stays in its own scene");Check(!QuizManager.Instance.IsQuizActive,"No stale reflection over final result");
        return "PASS remaining campaign: "+checks+" checks; chapters 3–5, renamed NPC routes, final result, choice and checkpoint reload.";
    }
    public static async Task<string> MenuAndInteraction()
    {
        checks=0;Check(Sequence!=null&&Sequence.ChapterNumber==5&&Sequence.IsChapterComplete,"Final result ready");Sequence.AdvanceChapter();await Wait(()=>UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>()!=null&&GameStateManager.Instance.CurrentState==GameState.MainMenu,"Return to menu");
        var menu=UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>();var settings=(GameObject)new SerializedObject(menu).FindProperty("settingsPanel").objectReferenceValue;var settingsButton=(Button)new SerializedObject(menu).FindProperty("settingsButton").objectReferenceValue;settingsButton.onClick.Invoke();Check(settings.activeSelf,"Settings button opens preferences");Check(settings.GetComponentsInChildren<Toggle>().Length==3,"Three accessibility settings");await Task.Delay(600);await Capture("MenuSettings");settingsButton.onClick.Invoke();
        var button=(Button)new SerializedObject(menu).FindProperty("newGameButton").objectReferenceValue;button.onClick.Invoke();await Wait(()=>Sequence!=null&&UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().IsRunning,"Actual menu new game opens Aji");await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Actual opening signal from menu");Lines();UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>().Skip();await Wait(()=>Sequence.ActiveQuest!=null,"Aji first objective");
        var npc=GameObject.Find("_Level/StoryCharacters/Denis").GetComponent<MissionDialogueNPC>();Place(npc.transform.position+Vector3.back*.85f);var detector=Player.GetComponent<BullyingGame.Interaction.InteractionDetector>();await Wait(()=>ReferenceEquals(detector.CurrentInteractable,npc),"Real trigger selects Denis");var gamepad=InputSystem.AddDevice<Gamepad>();
        try
        {
            InputSystem.QueueStateEvent(gamepad,new GamepadState().WithButton(GamepadButton.West));await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Actual gamepad interaction opens Denis dialogue");InputSystem.QueueStateEvent(gamepad,new GamepadState());await Task.Delay(250);
            var ui=UnityEngine.Object.FindAnyObjectByType<DialogueUI>();int guard=0;while(!DialogueManager.Instance.IsAwaitingChoice&&guard++<20){ui.SkipOrNext();await Task.Delay(40);}Check(DialogueManager.Instance.IsAwaitingChoice,"Real dialogue UI reaches choices");await Task.Delay(1200);await Capture("AjiChoices_UI");
            var choices=UnityEngine.Object.FindAnyObjectByType<DialogueChoiceUI>();var panel=(GameObject)new SerializedObject(choices).FindProperty("panel").objectReferenceValue;Check(panel.activeInHierarchy,"Visible choices panel");Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject!=null,"UI selection ready for gamepad");InputSystem.QueueStateEvent(gamepad,new GamepadState().WithButton(GamepadButton.South));await Wait(()=>!DialogueManager.Instance.IsAwaitingChoice,"Actual UI submit selects first response");InputSystem.QueueStateEvent(gamepad,new GamepadState());Lines();await Wait(()=>Sequence.ActiveQuest.questId=="ch01_route_support","UI selected supportive branch");
        }
        finally{InputSystem.RemoveDevice(gamepad);}
        return "PASS menu/interaction: "+checks+" checks, real menu buttons, settings, new game, trigger, gamepad interact and UI choice submit.";
    }
    public static async Task<string> ResumeCampaignAndMenu()
    {
        Check(Application.isPlaying&&SaveManager.Instance!=null,"Menu Play Mode setup");Application.runInBackground=true;typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(SaveManager.Instance,PathSave);SaveManager.Instance.LoadGame();
        Check(SaveManager.Instance.CurrentData.campaignCompleted,"Existing isolated campaign result");GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level01");await Wait(()=>Sequence!=null&&Sequence.IsChapterComplete,"Load isolated result before menu check");return await MenuAndInteraction();
    }
}

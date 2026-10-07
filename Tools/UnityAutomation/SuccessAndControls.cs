using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using TMPro;
using BullyingGame.Core;
using BullyingGame.Cinematics;
using BullyingGame.NPC;
using BullyingGame.Quest;
using BullyingGame.Dialogue;
using BullyingGame.Events;
using BullyingGame.QTE;
using BullyingGame.Save;
using BullyingGame.UI;

public static class SuccessAndControls
{
    static int checks;
    static void Check(bool condition,string name){if(!condition)throw new Exception("FAIL: "+name);checks++;}
    static async Task Wait(Func<bool> condition,string name,float seconds=14){double end=EditorApplication.timeSinceStartup+seconds;while(!condition() && EditorApplication.timeSinceStartup<end)await Task.Delay(15);Check(condition(),name);}
    static void FinishDialogue(){for(int i=0;i<40 && DialogueManager.Instance.IsDialogueActive;i++)DialogueManager.Instance.NextLine();Check(!DialogueManager.Instance.IsDialogueActive,"Normal dialogue completed");}
    static async Task Talk(string path)
    {
        var npc=GameObject.Find(path).GetComponent<MissionDialogueNPC>();
        UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>().Teleport(npc.transform.position+Vector3.back*.85f,Quaternion.identity);
        Physics.SyncTransforms();
        var detector=GameObject.Find("Player").GetComponent<BullyingGame.Interaction.InteractionDetector>();
        await Wait(()=>ReferenceEquals(detector.CurrentInteractable,npc),"Actual trigger detector selects "+npc.name);
        InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.E));
        await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Actual E interaction starts dialogue "+npc.name);
        InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
        await Task.Delay(650);
        UnityEngine.Object.FindAnyObjectByType<DialogueUI>().SkipOrNext();
        var body=(TextMeshProUGUI)new SerializedObject(UnityEngine.Object.FindAnyObjectByType<DialogueUI>()).FindProperty("dialogueText").objectReferenceValue;
        Check(body.isActiveAndEnabled && body.text.Length>20 && body.maxVisibleCharacters>20,"Readable visible dialogue text");
        if(npc.name=="Dimas"){ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/AutomationSuccess/Dialogue.png"));await Task.Delay(150);}
        FinishDialogue();await Task.Delay(50);
    }
    public static async Task<string> Run()
    {
        Check(Application.isPlaying,"Play Mode required");Application.runInBackground=true;var save=SaveManager.Instance;typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(save,Path.GetFullPath("Logs/AutomationSuccess/savegame.json"));save.ResetProgress();
        var sequence=UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();var chapterProperty=new SerializedObject(sequence).FindProperty("chapters");var quests=Enumerable.Range(0,chapterProperty.arraySize).SelectMany(i=>((NarrativeChapterData)chapterProperty.GetArrayElementAtIndex(i).objectReferenceValue).Missions).ToArray();var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();if(opening.IsRunning)opening.Skip();DialogueManager.Instance.EndDialogue();QuestManager.Instance.RestoreProgress(new QuestProgressRecord[0],quests);BullyingEventManager.Instance.RestoreHandledEvents(new string[0]);typeof(StoryChapterSequence).GetField("chapterIndex",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(sequence,0);GameStateManager.Instance.SetState(GameState.Playing);typeof(StoryChapterSequence).GetMethod("EnterChapter",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sequence,new object[]{false});await Wait(()=>sequence.ActiveQuest!=null,"First mission starts");
        var player=GameObject.Find("Player");var keyboard=Keyboard.current;bool ownsKeyboard=keyboard==null;if(ownsKeyboard)keyboard=InputSystem.AddDevice<Keyboard>();var initial=player.transform.position;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));await Task.Delay(300);Check(Vector3.Distance(initial,player.transform.position)>.4f,"Generated WASD input moves actual CharacterController");Check(player.GetComponentInChildren<Animator>().GetFloat("Speed")>.05f,"Movement drives locomotion Animator");InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Task.Delay(60);
        var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();pause.PauseGame();Check(GameStateManager.Instance.CurrentState==GameState.Paused && !player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Playing pause locks controls");pause.ResumeGame();Check(GameStateManager.Instance.CurrentState==GameState.Playing && player.GetComponent<BullyingGame.Player.PlayerMovement>().enabled,"Playing pause restores controls");
        await Talk("_Level/StoryCharacters/Dimas");await Wait(()=>sequence.ActiveQuest.questId!=quests[0].questId,"Second quest starts");await Talk("_Level/BullyGroup/Bully_Leader");var bookQuest=sequence.ActiveQuest;var item=UnityEngine.Object.FindAnyObjectByType<QuestItem>();UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>().Teleport(item.transform.position+Vector3.back,Quaternion.identity);item.Interact(player);await Wait(()=>DialogueManager.Instance.IsDialogueActive,"Actual confrontation");await Task.Delay(400);FinishDialogue();await Wait(()=>QTEManager.Instance.IsQTEActive,"Actual QTE ready");await Task.Delay(650);
        foreach(var npc in UnityEngine.Object.FindObjectsByType<BullyNPC>()){var p=UnityEngine.Camera.main.WorldToViewportPoint(npc.transform.position+Vector3.up*.15f);Check(p.y>.34f && p.y<.95f,"Actor feet clear QTE panel "+npc.name+" "+p);}
        var qteUI=UnityEngine.Object.FindAnyObjectByType<QTEUI>();var ui=new SerializedObject(qteUI);var panel=(GameObject)ui.FindProperty("qtePanel").objectReferenceValue;Check(!panel.GetComponentsInChildren<TextMeshProUGUI>().Any(t=>t.text=="PERTAHANKAN!"),"Duplicate QTE title removed");var timer=(UnityEngine.UI.Image)ui.FindProperty("timerBar").objectReferenceValue;Check(timer.transform.parent==panel.transform && timer.type==UnityEngine.UI.Image.Type.Filled && timer.sprite!=null,"Timer is directly positioned flat filled bar");
        ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/AutomationSuccess/CorrectedQTE.png"));var gamepad=InputSystem.AddDevice<Gamepad>();
        try{for(int i=0;i<10 && QTEManager.Instance.IsQTEActive;i++){InputSystem.QueueStateEvent(gamepad,new GamepadState().WithButton(GamepadButton.South));await Task.Delay(35);InputSystem.QueueStateEvent(gamepad,new GamepadState());await Task.Delay(35);}await Wait(()=>!QTEManager.Instance.IsQTEActive,"Gamepad binding completes real QTE");await Wait(()=>GameStateManager.Instance.CurrentState==GameState.Playing && CutsceneManager.Instance.CurrentDirector==null,"Success returns cleanly to gameplay",20);Check(QuestManager.Instance.IsObjectiveCompleted(bookQuest,"find_notebook"),"Success secures item objective");Check(QuestManager.Instance.GetQuestState(bookQuest)==QuestState.Active,"Success leaves turn-in pending");Check(BullyingEventManager.Instance.CaptureHandledEvents().Length==1,"Success encounter handled once");await Talk("_Level/BullyGroup/Bully_Leader");Check(QuestManager.Instance.GetQuestState(bookQuest)==QuestState.Completed,"Success turn-in completes book quest");}
        finally{InputSystem.RemoveDevice(gamepad);if(ownsKeyboard)InputSystem.RemoveDevice(keyboard);}
        var mobile=UnityEngine.Object.FindAnyObjectByType<MobileGameplayUI>();var mobileSo=new SerializedObject(mobile);mobileSo.FindProperty("previewInEditor").boolValue=true;mobileSo.ApplyModifiedPropertiesWithoutUndo();await Task.Delay(80);var movement=(GameObject)mobileSo.FindProperty("movementGroup").objectReferenceValue;Check(movement.activeInHierarchy,"Touch movement group available in preview");Check(movement.GetComponentsInChildren<UnityEngine.InputSystem.OnScreen.OnScreenStick>().Length==2,"Touch move and look sticks enabled");Check(movement.GetComponentsInChildren<UnityEngine.InputSystem.OnScreen.OnScreenButton>().Length==2,"Touch interact and sprint enabled");mobileSo.FindProperty("previewInEditor").boolValue=false;mobileSo.ApplyModifiedPropertiesWithoutUndo();
        Check(!UnityEngine.Object.FindAnyObjectByType<VideoCutscenePlayer>().TryPlayVideo(null),"Missing intro video safely declines playback");return "PASS success/control integration: "+checks+" checks; visible dialogue, WASD/Animator, pause, corrected QTE UI/composition, real gamepad success, turn-in, touch preview and missing-video guard.";
    }
}

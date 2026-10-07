using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using BullyingGame.Core;
using BullyingGame.Cinematics;
using BullyingGame.Quest;
using BullyingGame.Dialogue;
using BullyingGame.Save;
using BullyingGame.UI;

public static class MenuReloadSmoke
{
    static int checks;
    static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);checks++;}
    static async Task Wait(Func<bool> value,string name,float seconds=18){double end=EditorApplication.timeSinceStartup+seconds;while(!value() && EditorApplication.timeSinceStartup<end)await Task.Delay(20);Check(value(),name);}
    static T Field<T>(UnityEngine.Object owner,string field) where T:UnityEngine.Object => (T)new SerializedObject(owner).FindProperty(field).objectReferenceValue;
    static StoryChapterSequence Sequence => UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
    static MainMenuFlow Menu => UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>();
    static void SingletonCheck(){Check(UnityEngine.Object.FindObjectsByType<GameStateManager>().Length==1,"One game-state authority");Check(UnityEngine.Object.FindObjectsByType<SaveManager>().Length==1,"One persistent save manager");Check(UnityEngine.Object.FindObjectsByType<ChapterManager>().Length==1,"One persistent chapter manager");}
    public static string Prepare()
    {
        Check(!Application.isPlaying,"Edit Mode required");
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {var opened=EditorSceneManager.OpenScene(scene.path);foreach(var root in opened.GetRootGameObjects())foreach(var child in root.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject)==0,"No missing script in "+scene.path+" / "+child.name);}
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        return "PASS build-scene integrity: "+checks+" checks; Bootstrap ready for menu/reload test.";
    }
    public static string EnterBootstrap()
    {
        Check(Application.isPlaying,"Play Mode required");
        SceneManager.LoadScene("00_Bootstrap");
        return "Bootstrap reload requested inside existing Play session.";
    }
    public static string FreshLevel()
    {
        Check(Application.isPlaying,"Play Mode required");
        SaveManager.Instance.ResetProgress();
        SceneManager.LoadScene("Level01");
        return "Fresh Level reloaded inside the existing Play session and isolated save slot.";
    }
    public static async Task<string> Run()
    {
        Check(Application.isPlaying,"Play Mode required");Application.runInBackground=true;
        await Wait(()=>Menu!=null && GameStateManager.Instance.CurrentState==GameState.MainMenu,"Bootstrap loads MainMenu");await Task.Delay(120);SingletonCheck();
        string test=Path.GetFullPath("Logs/AutomationMenu/savegame.json");Directory.CreateDirectory(Path.GetDirectoryName(test));File.Copy(Path.GetFullPath("Logs/AutomationCampaign/savegame.json"),test,true);
        typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(SaveManager.Instance,test);SaveManager.Instance.LoadGame();
        var priorMenu=Menu;SceneManager.LoadScene("01_MainMenu");await Wait(()=>priorMenu==null && Menu!=null,"Menu reload refreshes continue status");await Task.Delay(120);
        var menu=Menu;var continueButton=Field<Button>(menu,"continueButton");Check(continueButton.interactable,"Continue enabled for saved campaign");
        var settings=Field<GameObject>(menu,"settingsPanel");Field<Button>(menu,"settingsButton").onClick.Invoke();Check(settings.activeSelf,"Settings opens");Field<Button>(menu,"settingsButton").onClick.Invoke();Check(!settings.activeSelf,"Settings closes");
        continueButton.onClick.Invoke();await Wait(()=>Sequence!=null && Sequence.IsChapterComplete,"Continue restores final Result");SingletonCheck();Check(Sequence.ChapterNumber==5 && POVManager.Instance.CurrentPOV==POVType.Witness,"Continue restores chapter and POV");Check(QuestManager.Instance.CaptureProgress().Count(q=>q.state==QuestState.Completed)==22,"All completed missions restored");Check(SaveManager.Instance.CurrentData.empathyScore==100,"Reflection points survive loading");Check(GameStateManager.Instance.CurrentState==GameState.Result,"Loading does not stomp restored Result");
        var quiz=UnityEngine.Object.FindAnyObjectByType<BullyingGame.Quiz.QuizUI>();Check(!Field<GameObject>(quiz,"panelRoot").activeInHierarchy,"Old quiz panel is hidden after continue");var presentation=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>();Check(Field<GameObject>(presentation,"endPanel").activeInHierarchy,"Final Result visible");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/AutomationMenu/ContinuedResult.png"));await Task.Delay(130);
        Field<Button>(presentation,"nextButton").onClick.Invoke();await Wait(()=>Menu!=null && GameStateManager.Instance.CurrentState==GameState.MainMenu,"Final button returns to Menu");await Task.Delay(120);SingletonCheck();ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/AutomationMenu/MainMenu.png"));await Task.Delay(120);
        Field<Button>(Menu,"newGameButton").onClick.Invoke();await Wait(()=>Sequence!=null && DialogueManager.Instance!=null && DialogueManager.Instance.IsDialogueActive,"New Game starts native Timeline signal dialogue");Check(SaveManager.Instance.CurrentData.empathyScore==0 && !SaveManager.Instance.CurrentData.campaignCompleted,"New Game resets isolated progress");while(DialogueManager.Instance.IsDialogueActive)DialogueManager.Instance.NextLine();await Wait(()=>Sequence.ActiveQuest!=null && GameStateManager.Instance.CurrentState==GameState.Playing,"New Game opening returns to gameplay",18);SingletonCheck();
        var first=Sequence.ActiveQuest;var dimas=GameObject.Find("_Level/StoryCharacters/Dimas").GetComponent<BullyingGame.NPC.MissionDialogueNPC>();UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>().Teleport(dimas.transform.position+Vector3.back*1.7f,Quaternion.identity);dimas.Interact(GameObject.Find("Player"));Check(DialogueManager.Instance.IsDialogueActive,"New campaign conversation starts");while(DialogueManager.Instance.IsDialogueActive)DialogueManager.Instance.NextLine();await Wait(()=>Sequence.ActiveQuest!=null && Sequence.ActiveQuest!=first,"New campaign advances objective");
        var nextQuest=Sequence.ActiveQuest;var playable=UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>();playable.Teleport(new Vector3(-3,.05f,2),Quaternion.identity);Check(UnityEngine.Object.FindAnyObjectByType<SceneCheckpoint>().CaptureNow(),"Checkpoint stores progress and pose");var position=GameObject.Find("Player").transform.position;var oldSequence=Sequence;var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();pause.PauseGame();pause.RestartLevel();await Wait(()=>oldSequence==null && Sequence!=null && Sequence.ActiveQuest!=null,"Pause restart reloads native Level scene");Check(GameStateManager.Instance.CurrentState==GameState.Playing && Time.timeScale==1,"Reload releases pause without stale cinematic");Check(QuestManager.Instance.GetQuestState(first)==QuestState.Completed && Sequence.ActiveQuest.questId==nextQuest.questId,"Reload preserves completed conversation and next quest");Check(Vector3.Distance(position,GameObject.Find("Player").transform.position)<.5f,"Reload restores valid player position");SingletonCheck();
        var loader=UnityEngine.Object.FindAnyObjectByType<LevelLoader>();if(loader==null)loader=new GameObject("TestLevelLoader").AddComponent<LevelLoader>();loader.LoadScene("nonexistent-test-scene");Check(!loader.IsLoading,"Invalid scene request safely ignored");loader.LoadScene("01_MainMenu");await Wait(()=>Menu!=null && GameStateManager.Instance.CurrentState==GameState.MainMenu && !loader.IsLoading,"LevelLoader preserves menu state after asynchronous load");SingletonCheck();
        return "PASS menu/bootstrap/reload: "+checks+" checks; actual menu buttons, final checkpoint, new game Timeline, pause reload, singleton lifecycle and scene loader.";
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using BullyingGame.Core;
using BullyingGame.Quest;
using BullyingGame.Quiz;
using BullyingGame.Save;
using BullyingGame.UI;
public static class ReflectionResumeSmoke
{
    static int checks;
    static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);checks++;}
    static async Task Wait(Func<bool> value,string name){double end=EditorApplication.timeSinceStartup+14;while(!value() && EditorApplication.timeSinceStartup<end)await Task.Delay(20);Check(value(),name);}
    static StoryChapterSequence Sequence => UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
    public static string RestoreCompletedCampaign()
    {
        var save=SaveManager.Instance;
        typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(save,Path.GetFullPath("Logs/AutomationCampaign/savegame.json"));
        save.LoadGame();GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level01");
        return "Restoring completed campaign through Loading state.";
    }
    public static async Task<string> Run()
    {
        Check(Application.isPlaying && Sequence.IsChapterComplete,"Start from completed campaign");var save=SaveManager.Instance;string campaign=Path.GetFullPath("Logs/AutomationCampaign/savegame.json");var fixture=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(campaign));var chapters=new SerializedObject(Sequence).FindProperty("chapters");var first=(NarrativeChapterData)chapters.GetArrayElementAtIndex(0).objectReferenceValue;var ids=first.Missions.Select(q=>q.questId).ToArray();fixture.quests=fixture.quests.Where(q=>ids.Contains(q.questId)).ToArray();var reflection=fixture.quests.First(q=>q.questId==ids.Last());reflection.state=QuestState.Active;reflection.amounts=reflection.amounts.Select(v=>0).ToArray();fixture.currentChapter=1;fixture.currentPOV=POVType.Victim;fixture.empathyScore=0;fixture.reflections=new ReflectionCheckpoint[0];fixture.campaignCompleted=false;fixture.hasPlayerPosition=false;
        string test=Path.GetFullPath("Logs/AutomationReflection/savegame.json");typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(save,test);typeof(SaveManager).GetProperty("CurrentData").SetValue(save,fixture);Check(save.TrySaveGame(),"Isolated reflection fixture saved");GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level01");await Wait(()=>QuizManager.Instance!=null && QuizManager.Instance.IsQuizActive,"First pending reflection resumes");
        var firstText=QuizManager.Instance.CurrentQuestion.questionText;QuizManager.Instance.SubmitAnswer(QuizManager.Instance.CurrentQuestion.correctOptionIndex);QuizManager.Instance.SubmitAnswer(QuizManager.Instance.CurrentQuestion.correctOptionIndex);Check(save.CurrentData.empathyScore==10,"First answer awarded once");QuizManager.Instance.CloseQuiz();await Wait(()=>QuizManager.Instance.IsQuizActive && QuizManager.Instance.CurrentQuestion.questionText!=firstText,"Second reflection starts");var secondText=QuizManager.Instance.CurrentQuestion.questionText;var disk=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(test));Check(disk.empathyScore==10 && disk.reflections.Length==1 && disk.reflections[0].closed,"Closed reflection checkpoint persists before next quiz");
        QuizManager.Instance.SubmitAnswer(QuizManager.Instance.CurrentQuestion.correctOptionIndex);Check(save.CurrentData.empathyScore==20,"Second question awards once");var prior=Sequence;var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();pause.PauseGame();pause.RestartLevel();await Wait(()=>prior==null && QuizManager.Instance!=null && QuizManager.Instance.IsQuizActive,"Pause reload resumes pending answer feedback flow");Check(QuizManager.Instance.CurrentQuestion.questionText==secondText,"Closed first question is skipped on resume");QuizManager.Instance.SubmitAnswer(QuizManager.Instance.CurrentQuestion.correctOptionIndex);Check(save.CurrentData.empathyScore==20,"Re-answer after reload never awards duplicate points");QuizManager.Instance.CloseQuiz();await Wait(()=>Sequence.IsChapterComplete,"Both reflections finish chapter");var quizUI=UnityEngine.Object.FindAnyObjectByType<QuizUI>();Check(!((GameObject)new SerializedObject(quizUI).FindProperty("panelRoot").objectReferenceValue).activeInHierarchy,"Manager close hides reflected UI");
        disk=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(test));Check(disk.empathyScore==20 && disk.reflections.Length==2 && disk.reflections.All(r=>r.closed),"Two completed reflections persist");save.LoadGame();prior=Sequence;GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level01");await Wait(()=>prior==null && Sequence!=null && Sequence.IsChapterComplete,"Disk checkpoint resumes result without repeated quiz");Check(!QuizManager.Instance.IsQuizActive && save.CurrentData.empathyScore==20,"Disk round trip preserves points and completion");
        typeof(SaveManager).GetField("saveFilePath",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(save,campaign);save.LoadGame();prior=Sequence;GameStateManager.Instance.SetState(GameState.Loading);SceneManager.LoadScene("Level01");await Wait(()=>prior==null && Sequence!=null && Sequence.ChapterNumber==5 && Sequence.IsChapterComplete,"Restore isolated completed-campaign fixture");
        return "PASS reflection checkpoint: "+checks+" checks; completed question skipping, immediate close save, pause reload, no repeated reward and disk restore.";
    }
}

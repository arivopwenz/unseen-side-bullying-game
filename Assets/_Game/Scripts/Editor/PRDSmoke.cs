using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.Quest;
using BullyingGame.NPC;
using BullyingGame.Save;
using BullyingGame.Quiz;
using BullyingGame.UI;
using BullyingGame.Cinematics;

namespace BullyingGame.EditorTools
{
    public static class PRDSmoke
    {
        public static string Status="Idle";
        public static int Checks;
        private static string reportPath="Logs/PRD11/Smoke.txt";
        private static void Check(bool condition,string message){if(!condition)throw new Exception(message);Checks++;File.AppendAllText(reportPath,"PASS "+message+"\n");}
        private static async Task Wait(Func<bool> test,string reason,int timeout=20000)
        {var until=DateTime.UtcNow.AddMilliseconds(timeout);while(!test()){if(DateTime.UtcNow>until)throw new Exception("Timeout: "+reason);await Task.Delay(35);}}
        public static string Prepare()
        {
            Directory.CreateDirectory("Logs/PRD11");
            SessionState.SetString("PRD.TestSavePath",Path.GetFullPath("Logs/PRD11/save-"+DateTime.UtcNow.Ticks+".json"));
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity");
            EditorApplication.isPlaying=true;return "Play requested with isolated save";
        }
        public static void Begin(int variant=0){if(Status.StartsWith("Running"))return;_=Run(variant);}
        private static async Task FinishDialogue(bool allowChoice=false)
        {
            for(int i=0;i<100;i++)
            {
                var d=DialogueManager.Instance;if(d==null||!d.IsDialogueActive||d.IsAwaitingChoice)return;
                d.NextLine();await Task.Delay(35);
            }
            throw new Exception("Dialogue did not end");
        }
        private static async Task Opening()
        {
            var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();
            await Wait(()=>opening!=null&&opening.IsRunning,"opening starts");opening.Skip();
            await Wait(()=>opening.IsShowingSkipSummary&&DialogueManager.Instance.IsDialogueActive,"readable skip summary");
            var pause=UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>();var before=DialogueManager.Instance.CurrentLine;pause.PauseGame();DialogueManager.Instance.NextLine();
            Check(GameStateManager.Instance.CurrentState==GameState.Paused&&DialogueManager.Instance.CurrentLine==before,"Opening summary can pause without advancing");pause.ResumeGame();await FinishDialogue();
            await Wait(()=>!opening.IsRunning&&GameStateManager.Instance.CurrentState==GameState.Playing,"opening restores control");
        }
        private static StoryChapterSequence Sequence=>UnityEngine.Object.FindAnyObjectByType<StoryChapterSequence>();
        private static async Task Run(int variant)
        {
            Status="Running campaign "+variant;Checks=0;reportPath="Logs/PRD11/Smoke-"+variant+".txt";File.WriteAllText(reportPath,"PRD 1.1 integration, variant "+variant+"\n");
            try
            {
                await Wait(()=>UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>()!=null,"bootstrap to menu");
                var menu=UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>();menu.NewGame();
                Check(StoryOverlay.Current!=null&&SceneManager.GetActiveScene().name=="01_MainMenu","New game requires confirmation");
                var confirm=StoryOverlay.Current.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name=="Mulai baru");confirm.onClick.Invoke();
                await Wait(()=>Sequence!=null,"load first chapter");
                var source=PRDContentAuthoring.Read();
                for(int chapter=1;chapter<=5;chapter++)
                {
                    await Opening();await Wait(()=>Sequence.ActiveQuest!=null,"first mission active");
                    Check(Sequence.ChapterNumber==chapter&&SceneManager.GetActiveScene().name=="Level0"+chapter,"Correct scene host chapter "+chapter);
                    Check(POVManager.Instance.CurrentPOV==POVType.Protagonist&&Sequence.CurrentChapter.PlayerName=="Aji","Aji remains playable chapter "+chapter);
                    var player=GameObject.Find("Player");
                    Check(player.transform.Find("RiggedVisual").gameObject.activeInHierarchy,"Aji visual active chapter "+chapter);
                    if(chapter==1)
                    {
                        var guide=UnityEngine.Object.FindAnyObjectByType<SchoolFieldGuide>();guide.OpenJournal("Bima");Check(StoryOverlay.Current!=null&&GameStateManager.Instance.CurrentState==GameState.Paused,"Journal opens and pauses");StoryOverlay.Current.Dismiss();Check(GameStateManager.Instance.CurrentState==GameState.Playing,"Journal returns control");guide.OpenMap();Check(StoryOverlay.Current!=null&&StoryOverlay.Current.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text=="● AJI"),"Map displays player location");StoryOverlay.Current.Dismiss();
                        var teacher=GameObject.Find("PRDCast/PakArif").GetComponent<MissionDialogueNPC>();teacher.Interact(player);Check(DialogueManager.Instance.IsDialogueActive,"Teacher help available before observations");await FinishDialogue();Check(Sequence.ActiveQuest.questId=="M01","Early assistance does not complete mission");
                        StorySettings.Show();Check(StoryOverlay.Current!=null,"Settings available");StoryOverlay.Current.Dismiss();
                        var save=SaveManager.Instance;
                        var pathField=typeof(SaveManager).GetField("saveFilePath",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                        var originalPath=(string)pathField.GetValue(save);
                        try
                        {
                            pathField.SetValue(save,Path.GetFullPath("Logs/PRD11"));
                            Check(!Sequence.GetComponent<SceneCheckpoint>().CaptureNow()&&StoryOverlay.Current!=null&&Time.timeScale==0,"Save write failure pauses and offers retry");
                        }
                        finally { pathField.SetValue(save,originalPath); }
                        StoryOverlay.Current.GetComponentsInChildren<UnityEngine.UI.Button>().First(b=>b.name=="Coba simpan lagi").onClick.Invoke();
                        Check(StoryOverlay.Current==null&&Time.timeScale==1&&save.LastError==null,"Save retry restores control and persists checkpoint");
                    }
                    foreach(var mission in source.missions.Skip((chapter-1)*3).Take(3))
                    {
                        await Wait(()=>Sequence.ActiveQuest!=null&&Sequence.ActiveQuest.questId==mission.id,"active "+mission.id);
                        Status="Running "+mission.id+" variant "+variant;
                        var quest=PRDContentAuthoring.Load<QuestData>("Quest_"+mission.id);
                        if(mission.id=="M15")
                        {
                            for(int qi=0;qi<6;qi++)
                            {
                                await Wait(()=>QuizManager.Instance.IsQuizActive&&!QuizManager.Instance.IsAnswered,"reflection "+qi);
                                var quiz=QuizManager.Instance;int wrong=(quiz.CurrentQuestion.correctOptionIndex+1)%3;
                                quiz.SubmitAnswer(wrong);Check(quiz.IsAnswered,"Wrong answer has feedback");
                                if(variant==0){quiz.RetryAnswer();quiz.SubmitAnswer(quiz.CurrentQuestion.correctOptionIndex);}
                                quiz.CloseQuiz();await Task.Delay(100);
                            }
                            await Wait(()=>QuestManager.Instance.GetQuestState(quest)==QuestState.Completed,"reflection mission completed");
                            Check(SaveManager.Instance.CurrentData.reflections.Length==6&&SaveManager.Instance.CurrentData.reflections.All(r=>r.closed&&!r.correct&&(variant!=0||r.retries.Length==1)),"Six immutable first answers and separate retries saved; wrong answers do not gate ending");
                            continue;
                        }
                        foreach(var step in mission.steps)
                        {
                            var npc=GameObject.Find("PRDCast/"+step.actor).GetComponent<MissionDialogueNPC>();
                            Check(npc.OffersObjective(quest),mission.id+" offers "+step.id);
                            var destination=npc.transform.position+new Vector3(0,0,-2);
                            Check(NavMesh.SamplePosition(destination,out var hit,3,NavMesh.AllAreas),"Walkable interaction "+mission.id+"/"+step.id);
                            var controller=player.GetComponent<CharacterController>();controller.enabled=false;player.transform.position=hit.position;controller.enabled=true;Physics.SyncTransforms();
                            npc.Interact(player);Check(DialogueManager.Instance.IsDialogueActive,"Actual NPC interaction starts "+mission.id+"/"+step.id);
                            if(mission.id=="M13")
                            {
                                var presentation=Sequence.GetComponent<BullyingGame.Events.SchoolStoryPresentation>();
                                Check(presentation.CanSkipPrivateScene&&player.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),"Private context excludes Aji and allows skip");
                                presentation.SkipPrivateScene();
                                Check(DialogueManager.Instance.IsDialogueActive&&DialogueManager.Instance.CurrentLine.speakerName=="Ringkasan"&&!QuestManager.Instance.IsObjectiveCompleted(quest,step.id),"M13 skip requires reading summary before completion");
                            }
                            if(mission.id=="M01"&&step==mission.steps[0])
                            {
                                DialogueManager.Instance.EndDialogue();
                                Check(!QuestManager.Instance.IsObjectiveCompleted(quest,step.id),"Canceled dialogue cannot complete objective");
                                npc.Interact(player);
                            }
                            await FinishDialogue();
                            var dm=DialogueManager.Instance;
                            if(dm.IsAwaitingChoice)
                            {
                                var choices=dm.CurrentDialogue.Responses;int unsafeIndex=Array.FindIndex(choices,c=>c.RetryChoices);
                                if(unsafeIndex>=0)
                                {
                                    dm.SelectChoice(unsafeIndex);await FinishDialogue();Check(dm.IsAwaitingChoice&&!QuestManager.Instance.IsObjectiveCompleted(quest,step.id),"Unsafe response retries without mission reward "+mission.id+"/"+step.id);
                                }
                                int selected=variant==0?0:Math.Min(variant,choices.Length-1);if(choices[selected].RetryChoices)selected=choices.Length>2?1:0;
                                dm.SelectChoice(selected);await FinishDialogue();
                            }
                            Check(QuestManager.Instance.IsObjectiveCompleted(quest,step.id),"Safe/consent branch completes "+mission.id+"/"+step.id);
                            int entries=SaveManager.Instance.CurrentData.journalEntries.Length;QuestManager.Instance.UpdateObjective(quest,step.id);Check(SaveManager.Instance.CurrentData.journalEntries.Length==entries,"Repeated event cannot duplicate journal "+mission.id+"/"+step.id);
                            if(mission.id=="M13")Check(GameObject.Find("PrivateFollowUpStage")==null,"Private scene ends and restores Aji");
                            await Task.Delay(100);
                            if(mission.id=="M11"&&step.id=="retrieve")
                            {
                                Check(Sequence.GetComponent<SceneCheckpoint>().CaptureNow(),"Bag recovery checkpoint saved mid-mission");
                                int journalCount=SaveManager.Instance.CurrentData.journalEntries.Length;
                                UnityEngine.Object.FindAnyObjectByType<PauseMenuUI>().ReturnToMenu();
                                await Wait(()=>UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>()!=null,"return menu mid-mission");
                                UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>().ContinueGame();
                                await Wait(()=>Sequence!=null&&Sequence.ActiveQuest!=null,"continue mid-mission");
                                player=GameObject.Find("Player");
                                Check(Sequence.ChapterNumber==4&&Sequence.ActiveQuest.questId=="M11"&&QuestManager.Instance.IsObjectiveCompleted(quest,"retrieve")&&!QuestManager.Instance.IsObjectiveCompleted(quest,"return"),"Continue restores bag objective without replaying retrieval");
                                Check(SaveManager.Instance.CurrentData.journalEntries.Length==journalCount,"Continue preserves journal without duplicates");
                                await Task.Delay(100);
                                Check(Vector3.Distance(GameObject.Find("RakaBag").transform.position,GameObject.Find("PRDCast/BuRatih").transform.position)<2,"Recovered bag remains with teacher after continue");
                            }
                        }
                        await Wait(()=>QuestManager.Instance.GetQuestState(quest)==QuestState.Completed,mission.id+" complete");
                    }
                    await Wait(()=>Sequence.IsChapterComplete,"chapter result");Check(Sequence.GetComponent<SceneCheckpoint>().CaptureNow(),"Chapter checkpoint saved "+chapter);
                    var old=Sequence;old.AdvanceChapter();
                    if(chapter<5)await Wait(()=>old==null&&Sequence!=null&&Sequence.ChapterNumber==chapter+1,"chapter scene transition");
                }
                await Wait(()=>UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>()!=null,"epilog returns menu");
                Check(SaveManager.Instance.CurrentData.quests.Count(q=>q.state==QuestState.Completed)==15,"15/15 missions completed");
                Check(SaveManager.Instance.CurrentData.completedChapters.Length==5&&SaveManager.Instance.CurrentData.campaignCompleted,"All chapters and epilog saved");
                var path=SessionState.GetString("PRD.TestSavePath","");var saved=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));Check(SaveManager.Validate(saved),"Saved JSON validates");
                var first=saved.reflections.Select(r=>r.firstAnswer).ToArray();SaveManager.Instance.LoadGame();Check(first.SequenceEqual(SaveManager.Instance.CurrentData.reflections.Select(r=>r.firstAnswer)),"First-answer round trip");
                // Corrupt only the isolated fixture; never the player's save.
                File.WriteAllText(path,"{broken");SaveManager.Instance.LoadGame();Check(SaveManager.Instance.HasSave&&SaveManager.Validate(SaveManager.Instance.CurrentData),"Corrupt primary recovers valid backup");
                File.AppendAllText(reportPath,"PASS TOTAL "+Checks+"\n");Status="PASS "+Checks+" checks variant "+variant;
            }
            catch(Exception e){Status="FAIL "+e.Message;File.AppendAllText(reportPath,"FAIL "+e+"\n");Debug.LogException(e);}
        }
        public static string Integrity()
        {
            Directory.CreateDirectory("Logs/PRD11");reportPath="Logs/PRD11/Integrity.txt";File.WriteAllText(reportPath,"PRD scene/data integrity\n");Checks=0;
            var source=PRDContentAuthoring.Read();Check(source.missions.Length==15&&source.chapters.Length==5,"PRD counts");
            foreach(var m in source.missions)Check(QuestManager.IsValid(PRDContentAuthoring.Load<QuestData>("Quest_"+m.id)),"Valid quest "+m.id);
            for(int chapter=1;chapter<=5;chapter++)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Level_0"+chapter+"/Level0"+chapter+".unity");
                var seq=Sequence;Check(seq!=null,"Sequence in chapter "+chapter);var s=new SerializedObject(seq);Check(s.FindProperty("sceneChapterNumber").intValue==chapter,"Scene host "+chapter);
                Check(GameObject.FindGameObjectsWithTag("Player").Length==1,"Single Aji "+chapter);
                var environment=GameObject.Find("SchoolEnvironment");Check(environment!=null&&PrefabUtility.IsPartOfPrefabInstance(environment),"Shared school prefab "+chapter);
                Check(environment.GetComponent<Unity.AI.Navigation.NavMeshSurface>().navMeshData!=null,"Baked navigation "+chapter);
                Check(UnityEngine.Object.FindObjectsByType<StoryOpeningDirector>(FindObjectsSortMode.None).Length==1,"Opening active "+chapter);
                foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"No missing script: "+t.name);
            }
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity");return "PASS "+Checks;
        }
    }
}

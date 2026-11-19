using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BullyingGame.Prologue;

[InitializeOnLoad]
static class PrologueMapSessionQA
{
    const string Key="PrologueQA.Phase", Report="Logs/FinalCampaignPolishQA.txt";
    static Gamepad pad;
    static int phase,frame=-1,wait;
    static double deadline;
    static Vector3 initial;
    static PrologueMapSessionQA()=>EditorApplication.update+=Tick;
    static void Tick()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating)return;
        var session=SessionState.GetString(Key,"");
        if(!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if(session=="restore")
            {
                SessionState.EraseString(Key);
                SessionState.EraseString("PRD.TestSavePath");
                EditorSceneManager.OpenScene(SessionState.GetString("PrologueQA.Scene","Assets/_Game/Scenes/Levels/Level_01/Level01.unity"));
                File.AppendAllText(Report,"Map test restored editor.\n");
            }
            else if(File.Exists("Temp/PrologueMapQA.request") && session=="")
            {
                File.Delete("Temp/PrologueMapQA.request");
                phase=0;wait=0;frame=-1;
                var s=SceneManager.GetActiveScene();
                if(s.isDirty || SceneManager.sceneCount!=1){File.AppendAllText(Report,"Map test blocked by unsaved scene.\n");return;}
                SessionState.SetString("PrologueQA.Scene",s.path);
                SessionState.SetString("PRD.TestSavePath",Path.GetFullPath("Logs/MapPadTest.json"));
                SessionState.SetString(Key,"run");
                EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Prologue/Tutorial.unity");
                EditorApplication.isPlaying=true;
            }
            return;
        }
        if(session!="run" || !EditorApplication.isPlaying || Time.frameCount==frame)return;
        frame=Time.frameCount;
        if(wait-->0)return;
        try
        {
            var flow=UnityEngine.Object.FindFirstObjectByType<PrologueFlow>();
            if(flow==null || Time.frameCount<8)return;
            var obj=new SerializedObject(flow);
            var player=(Component)obj.FindProperty("player").objectReferenceValue;
            var hud=(Component)obj.FindProperty("hud").objectReferenceValue;
            var canvas=(Canvas)new SerializedObject(hud).FindProperty("canvas").objectReferenceValue;
            const BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
            var stage=typeof(PrologueFlow).GetField("stage",flags);
            bool open=(bool)typeof(PrologueFlow).GetField("mapOpen",flags).GetValue(flow);
            if(phase>0 && EditorApplication.timeSinceStartup>deadline)throw new Exception("Timeout "+phase+" stage "+stage.GetValue(flow)+" focus "+EventSystem.current?.currentSelectedGameObject?.name);
            switch(phase)
            {
                case 0:
                    pad=InputSystem.AddDevice<Gamepad>();
                    initial=player.transform.position;
                    InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.up});
                    deadline=EditorApplication.timeSinceStartup+70;
                    phase++;break;
                case 1:
                    if(Vector3.Distance(initial,player.transform.position)<.45f || !canvas.GetComponentsInChildren<Text>(true).Any(t=>t.text.Contains("Stik kiri")))return;
                    File.AppendAllText(Report,"Virtual stick moved Aji and changed the tutorial labels.\n");
                    InputSystem.QueueStateEvent(pad,new GamepadState());
                    typeof(PrologueFlow).GetMethod("SetStage",flags).Invoke(flow,new[]{Enum.Parse(stage.FieldType,"SelectCanteen")});
                    phase++;wait=2;break;
                case 2: InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.Select));phase++;break;
                case 3:
                    if(!open || EventSystem.current?.currentSelectedGameObject==null)return;
                    if(!EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text.Contains("Kantin"))throw new Exception("Wrong map focus.");
                    InputSystem.QueueStateEvent(pad,new GamepadState());phase++;wait=2;break;
                case 4: InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));phase++;break;
                case 5:
                    if(open || stage.GetValue(flow).ToString()!="GoToCanteen")return;
                    File.AppendAllText(Report,"GPS opened with Select; gamepad Submit selected Kantin and resumed travel.\n");Finish();break;
            }
        }catch(Exception e){File.AppendAllText(Report,e+"\n");Finish();}
    }
    static void Finish(){if(pad!=null)InputSystem.RemoveDevice(pad);SessionState.SetString(Key,"restore");EditorApplication.isPlaying=false;}
}

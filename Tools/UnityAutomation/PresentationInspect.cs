using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using BullyingGame.Cinematics;
using BullyingGame.Dialogue;
using BullyingGame.UI;
using BullyingGame.Quiz;
public static class PresentationInspect
{
 static string Path(Transform t)=>t.parent==null?t.name:Path(t.parent)+"/"+t.name;
 public static string Run()
 {
 var b=new StringBuilder();b.AppendLine("SCENE "+SceneManager.GetActiveScene().name);
 foreach(var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))b.AppendLine("CANVAS "+Path(c.transform)+" order="+c.sortingOrder);
 foreach(var obj in new UnityEngine.Object[]{UnityEngine.Object.FindAnyObjectByType<QuizUI>(),UnityEngine.Object.FindAnyObjectByType<QuestHUDUI>(),UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>(),UnityEngine.Object.FindAnyObjectByType<DialogueUI>(),UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>()})
 {b.AppendLine(obj.GetType().Name);var so=new SerializedObject(obj);var p=so.GetIterator();while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.ObjectReference&&p.objectReferenceValue is Component co)b.AppendLine(p.propertyPath+"="+Path(co.transform));else if(p.propertyType==SerializedPropertyType.ObjectReference&&p.objectReferenceValue is GameObject go)b.AppendLine(p.propertyPath+"="+Path(go.transform));}
 var opening=UnityEngine.Object.FindAnyObjectByType<StoryOpeningDirector>();var director=(PlayableDirector)new SerializedObject(opening).FindProperty("timeline").objectReferenceValue;var asset=(TimelineAsset)director.playableAsset;
 foreach(var track in asset.GetOutputTracks()){b.AppendLine("TRACK "+track.name+" "+track.GetType().Name);foreach(var clip in track.GetClips())b.AppendLine("CLIP "+clip.start+" "+clip.duration+" "+clip.asset.GetType().Name);}
 var d=AssetDatabase.LoadAssetAtPath<DialogueData>("Assets/_Game/Data/StoryPrototype/Opening_Victim.asset");foreach(var line in d.lines)b.AppendLine(line.speakerName+": "+line.text);
 return b.ToString();
 }
}

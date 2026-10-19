using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BullyingGame.Core;
using BullyingGame.Dialogue;
using BullyingGame.Quest;
using BullyingGame.NPC;
using BullyingGame.Quiz;

namespace BullyingGame.EditorTools
{
    public static class PRDContentAuthoring
    {
        public const string Root="Assets/_Game/Data/PRD/";
        [Serializable] public class Source { public Chapter[] chapters; public Mission[] missions; public Reflection[] reflections; }
        [Serializable] public class Chapter { public string title,summary,closing; public string[] opening; }
        [Serializable] public class Mission { public string id,title; public Step[] steps; }
        [Serializable] public class Step
        {
            public string id,actor,goal,requires,caseId,status,journal,safe,alternative,third,@unsafe,feedback;
            public string[] lines,response,alternativeResponse,thirdResponse;
        }
        [Serializable] public class Reflection { public string question; public string[] options,explanations; public int correct; }
        public static Source Read()=>JsonUtility.FromJson<Source>(File.ReadAllText(Root+"Campaign.json"));
        public static T Load<T>(string name) where T:UnityEngine.Object=>AssetDatabase.LoadAssetAtPath<T>(Root+name+".asset");
        private static T Asset<T>(string name) where T:ScriptableObject
        {
            var a=Load<T>(name);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,Root+name+".asset");}return a;
        }
        public static void Edit(UnityEngine.Object o,Action<SerializedObject> edit){var s=new SerializedObject(o);edit(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
        public static void Ref(UnityEngine.Object o,string field,UnityEngine.Object value)=>Edit(o,s=>s.FindProperty(field).objectReferenceValue=value);
        public static void Refs(UnityEngine.Object o,string field,UnityEngine.Object[] values)=>Edit(o,s=>{var p=s.FindProperty(field);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];});
        public static DialogueLine[] Lines(string[] lines)=>lines==null?Array.Empty<DialogueLine>():lines.Select(l=>{int i=l.IndexOf('|');return new DialogueLine{speakerName=i<0?"Narasi":l.Substring(0,i),text=i<0?l:l.Substring(i+1)};}).ToArray();
        private static DialogueData Dialogue(string id,string[] lines)
        {
            var d=Asset<DialogueData>("Dialogue_"+id);d.lines=Lines(lines);
            Edit(d,s=>{s.FindProperty("choiceId").stringValue="";s.FindProperty("responses").arraySize=0;s.FindProperty("contextResponses").arraySize=0;});return d;
        }
        private static void Choice(SerializedProperty p,string label,string[] lines,bool retry=false)
        {
            p.FindPropertyRelative("label").stringValue=label;p.FindPropertyRelative("retryChoices").boolValue=retry;
            var l=p.FindPropertyRelative("lines");var source=Lines(lines);l.arraySize=source.Length;
            for(int i=0;i<source.Length;i++){var v=l.GetArrayElementAtIndex(i);v.FindPropertyRelative("speakerName").stringValue=source[i].speakerName;v.FindPropertyRelative("text").stringValue=source[i].text;}
        }
        [MenuItem("Tools/The Unseen Side/Build PRD 1.1 Content")]
        public static void Build()
        {
            var source=Read();
            if(source.chapters.Length!=5||source.missions.Length!=15||source.reflections.Length!=6)throw new Exception("PRD content counts invalid");
            Dialogue("SkipM13",new[]{"Ringkasan|Aji tidak hadir dalam percakapan privat ini. Latar keluarga Dion bukan pembenaran dan bukan penyebab tunggal perundungan. Dion, Tio dan Fajar tetap bertanggung jawab menghentikan perilaku serta memulihkan kerugian.","Ringkasan|Pak Arif menetapkan konsekuensi edukatif dan tindak lanjut bersama keluarga. Dukungan untuk pelaku tidak mengurangi perlindungan korban. Korban tidak wajib memaafkan; isi konseling tidak menjadi catatan Aji."});
            QuestData previous=null;
            foreach(var m in source.missions)
            {
                var q=Asset<QuestData>("Quest_"+m.id);q.questId=m.id;q.questTitle=m.id+" · "+m.title;q.questDescription=m.title;
                q.objectives=m.steps.Select(s=>new QuestObjective{objectiveId=s.id,description=s.goal,requiredAmount=1}).ToArray();q.prerequisites=previous==null?Array.Empty<QuestData>():new[]{previous};previous=q;EditorUtility.SetDirty(q);
                foreach(var step in m.steps)
                {
                    if(string.IsNullOrEmpty(step.actor))continue;
                    var d=Dialogue(m.id+"_"+step.id,step.lines);
                    if(!string.IsNullOrEmpty(step.safe))Edit(d,s=>
                    {
                        s.FindProperty("choiceId").stringValue=m.id+"/"+step.id;
                        var choices=s.FindProperty("responses");int count=1+(string.IsNullOrEmpty(step.alternative)?0:1)+(string.IsNullOrEmpty(step.@unsafe)&&string.IsNullOrEmpty(step.third)?0:1);choices.arraySize=count;
                        int i=0;Choice(choices.GetArrayElementAtIndex(i++),step.safe,step.response);
                        if(!string.IsNullOrEmpty(step.alternative))Choice(choices.GetArrayElementAtIndex(i++),step.alternative,step.alternativeResponse);
                        if(!string.IsNullOrEmpty(step.@unsafe))Choice(choices.GetArrayElementAtIndex(i),step.@unsafe,new[]{"Narasi|"+step.feedback+" Coba pilih respons lain."},true);
                        else if(!string.IsNullOrEmpty(step.third))Choice(choices.GetArrayElementAtIndex(i),step.third,step.thirdResponse);
                    });
                }
            }
            for(int i=0;i<5;i++)
            {
                int index=i;var c=Asset<NarrativeChapterData>("Chapter0"+(i+1));var opening=Dialogue("Opening0"+(i+1),source.chapters[i].opening);Dialogue("Skip0"+(i+1),new[]{"Ringkasan|"+source.chapters[i].summary});
                Edit(c,s=>{s.FindProperty("chapterTitle").stringValue=source.chapters[index].title;s.FindProperty("playerName").stringValue="Aji";s.FindProperty("perspective").enumValueIndex=(int)POVType.Protagonist;s.FindProperty("allowsDialogueChoices").boolValue=true;s.FindProperty("sceneName").stringValue="Level0"+(index+1);s.FindProperty("closingMessage").stringValue=source.chapters[index].closing;s.FindProperty("openingDialogue").objectReferenceValue=opening;s.FindProperty("introVideo").objectReferenceValue=null;
                    var questions=s.FindProperty("reflections");questions.arraySize=index==4?6:0;
                    if(index==4)for(int j=0;j<6;j++){var q=questions.GetArrayElementAtIndex(j);var r=source.reflections[j];q.FindPropertyRelative("questionText").stringValue=r.question;q.FindPropertyRelative("correctOptionIndex").intValue=r.correct;q.FindPropertyRelative("educationalExplanation").stringValue=r.explanations[r.correct];Strings(q.FindPropertyRelative("options"),r.options);Strings(q.FindPropertyRelative("optionExplanations"),r.explanations);}
                });
                Refs(c,"missions",source.missions.Skip(i*3).Take(3).Select(m=>(UnityEngine.Object)Load<QuestData>("Quest_"+m.id)).ToArray());
            }
            string[] actors={"Sari","Bima","Laras","Raka","Dion","Tio","Fajar","PakBeni","BuRatih","PakArif","Koridor","FollowUp","AyahAji","IbuAji"};
            string[] names={"Sari","Bima","Laras","Raka","Dion","Tio","Fajar","Pak Beni","Bu Ratih","Pak Arif","Penanda koridor","Tindak lanjut sekolah","Ayah Aji","Ibu Aji"};
            for(int i=0;i<actors.Length;i++)
            {
                var n=Asset<NPCData>("NPC_"+actors[i]);n.npcName=names[i];n.interactionPrompt=actors[i]=="FollowUp"?"Lihat adegan":actors[i]=="Koridor"?"Baca penanda":"Bicara";EditorUtility.SetDirty(n);
                string message=i>=7&&i<=9?"Kamu boleh meminta bantuan sekarang walaupun jurnal belum lengkap atau misi lain belum selesai. Kita bisa bicara privat; saya dapat mendampingi ke ruang aman dan mengoordinasikan bantuan dengan guru terkait. Kamu tidak harus menghadapi pelaku sendiri.":actors[i]=="Sari"?"Lihat tujuan aktif atau buka peta. Aku dapat menunjukkan arah. Guru selalu dapat dimintai bantuan; tidak perlu menunggu semua catatan.":"Aku ada di sini. Kita bisa berbicara ketika sesuai kebutuhan cerita. Kamu boleh memberi ruang dan meminta bantuan guru.";
                Dialogue("Fallback_"+actors[i],new[]{names[i]+"|"+message});
            }
            AssetDatabase.SaveAssets();
        }
        private static void Strings(SerializedProperty p,string[] values){p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).stringValue=values[i];}
    }
}


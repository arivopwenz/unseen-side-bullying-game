using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BullyingGame.Dialogue;
using BullyingGame.QTE;
using BullyingGame.UI;
public static class InterfacePolish
{
    static GameObject Field(UnityEngine.Object obj,string name){var o=new SerializedObject(obj).FindProperty(name).objectReferenceValue;return o is GameObject g?g:o is Component c?c.gameObject:null;}
    static void Rect(GameObject go,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
    {go.transform.SetParent(parent,false);go.SetActive(true);var r=go.GetComponent<RectTransform>();r.localScale=Vector3.one;r.localRotation=Quaternion.identity;r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;var t=go.GetComponent<TextMeshProUGUI>();if(t!=null){t.enabled=true;t.margin=Vector4.zero;t.enableAutoSizing=false;t.color=new Color(.93f,.97f,1);}}
    static Sprite White()
    {const string path="Assets/_Game/Art/Prototype/UIWhite.asset";var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(sprite!=null)return sprite;var texture=new Texture2D(8,8,TextureFormat.RGBA32,false);texture.SetPixels(Enumerable.Repeat(Color.white,64).ToArray());texture.Apply();AssetDatabase.CreateAsset(texture,path);sprite=Sprite.Create(texture,new Rect(0,0,8,8),new Vector2(.5f,.5f));sprite.name="White";AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;}
    static void Background(Transform parent,string name,Vector2 pos,Vector2 size,Sprite sprite)
    {var t=parent.Find(name);var g=t!=null?t.gameObject:new GameObject(name,typeof(RectTransform),typeof(Image));Rect(g,parent,new Vector2(.5f,1),new Vector2(.5f,1),pos,size);var image=g.GetComponent<Image>();image.sprite=sprite;image.type=Image.Type.Simple;image.color=new Color(.18f,.28f,.32f,1);image.raycastTarget=false;g.transform.SetAsFirstSibling();g.SetActive(true);}
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        var dialogue=UnityEngine.Object.FindAnyObjectByType<DialogueUI>();var panel=Field(dialogue,"dialoguePanel");var speaker=Field(dialogue,"speakerNameText");var words=Field(dialogue,"dialogueText");var next=Field(dialogue,"continueButton");
        Rect(speaker,panel.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(32,-20),new Vector2(1490,38));speaker.GetComponent<TextMeshProUGUI>().fontSize=27;
        Rect(words,panel.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(32,-70),new Vector2(1490,100));words.GetComponent<TextMeshProUGUI>().fontSize=26;
        Rect(next,panel.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(-25,15),new Vector2(230,42));foreach(var text in next.GetComponentsInChildren<TextMeshProUGUI>(true)){text.transform.localScale=Vector3.one;text.fontSize=20;text.text="Lanjut";}
        var qte=UnityEngine.Object.FindAnyObjectByType<QTEUI>();var qtePanel=Field(qte,"qtePanel");var fields=new[]{"instructionText","timerText","progressText","resultText","timerBar","progressBar"};var objects=fields.Select(f=>Field(qte,f)).ToArray();
        foreach(var g in objects)g.transform.SetParent(qtePanel.transform,false);
        foreach(var child in Enumerable.Range(0,qtePanel.transform.childCount).Select(i=>qtePanel.transform.GetChild(i)).ToArray())if(!objects.Contains(child.gameObject))child.gameObject.SetActive(false);
        var sprite=White();qtePanel.GetComponent<Image>().sprite=sprite;qtePanel.GetComponent<Image>().color=new Color(.04f,.1f,.13f,.98f);
        Rect(objects[0],qtePanel.transform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-22),new Vector2(640,62));objects[0].GetComponent<TextMeshProUGUI>().fontSize=26;
        Rect(objects[1],qtePanel.transform,new Vector2(1,1),new Vector2(1,1),new Vector2(-20,-102),new Vector2(115,33));objects[1].GetComponent<TextMeshProUGUI>().fontSize=23;
        Rect(objects[2],qtePanel.transform,new Vector2(1,1),new Vector2(1,1),new Vector2(-20,-169),new Vector2(115,33));objects[2].GetComponent<TextMeshProUGUI>().fontSize=23;
        Rect(objects[3],qtePanel.transform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-234),new Vector2(640,45));objects[3].GetComponent<TextMeshProUGUI>().fontSize=25;objects[3].GetComponent<TextMeshProUGUI>().text="";
        for(int i=0;i<2;i++){var pos=new Vector2(-55,i==0?-107:-174);Rect(objects[4+i],qtePanel.transform,new Vector2(.5f,1),new Vector2(.5f,1),pos,new Vector2(470,25));var image=objects[4+i].GetComponent<Image>();image.sprite=sprite;image.type=Image.Type.Filled;image.fillMethod=Image.FillMethod.Horizontal;image.fillOrigin=0;image.fillAmount=i==0?1:0;image.color=i==0?new Color(.98f,.7f,.27f):new Color(.38f,.83f,.70f);Background(qtePanel.transform,i==0?"TimeBackground":"ProgressBackground",pos,new Vector2(470,25),sprite);objects[4+i].transform.SetAsLastSibling();}
        var so=new SerializedObject(qte);so.FindProperty("successColor").colorValue=new Color(.43f,.9f,.72f);so.FindProperty("failureColor").colorValue=new Color(1,.75f,.4f);so.ApplyModifiedPropertiesWithoutUndo();
        foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))foreach(var group in canvas.GetComponents<CanvasGroup>())group.alpha=1;
        var presentation=UnityEngine.Object.FindAnyObjectByType<ChapterPresentationUI>();presentation.GetComponent<Canvas>().sortingOrder=80;
        foreach(var name in new[]{"endPanel"}){var p=Field(presentation,name);p.GetComponent<Image>().sprite=sprite;p.GetComponent<Image>().color=new Color(.04f,.1f,.13f,1);}
        var quiz=UnityEngine.Object.FindAnyObjectByType<BullyingGame.Quiz.QuizUI>();var root=Field(quiz,"panelRoot");root.GetComponent<Image>().sprite=sprite;root.GetComponent<Image>().color=new Color(.04f,.1f,.13f,1);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(panel.scene);EditorSceneManager.SaveScene(panel.scene);
        return "Saved flat filled timer/progress bars, removed duplicate QTE heading, normalized text scales, opaque modal panels and ordering.";
    }
}

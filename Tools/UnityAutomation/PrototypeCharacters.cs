using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.Animations.Rigging;
using TMPro;
using BullyingGame.Player;
using BullyingGame.Core;
using BullyingGame.UI;
using BullyingGame.Audio;

public static class PrototypeCharacters
{
    const string Art="Assets/_Game/Art/Prototype/";
    const string Sound="Assets/_Game/Audio/Prototype/";
    static T C<T>(GameObject go) where T:Component { var c=go.GetComponent<T>();if(c==null)c=go.AddComponent<T>();return c; }
    static void Folder(string path) { if(AssetDatabase.IsValidFolder(path))return;var parent=Path.GetDirectoryName(path).Replace((char)92,(char)47);Folder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path)); }
    static GameObject Child(Transform parent,string name,Vector3 position) { var t=parent.Find(name);var g=t!=null?t.gameObject:new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;return g; }
    static Material Mat(string name,Color color) { var m=AssetDatabase.LoadAssetAtPath<Material>(Art+name+".mat");if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,Art+name+".mat");}m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.12f);EditorUtility.SetDirty(m);return m; }
    static void Ref(UnityEngine.Object obj,string name,UnityEngine.Object value){var so=new SerializedObject(obj);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj);}
    static void Mesh(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material,Quaternion rotation)
    {var t=parent.Find(name);var g=t!=null?t.gameObject:GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.transform.localRotation=rotation;var collider=g.GetComponent<Collider>();if(collider!=null)UnityEngine.Object.DestroyImmediate(collider);g.GetComponent<Renderer>().sharedMaterial=material;g.GetComponent<Renderer>().enabled=true;}
    static void Segment(Transform root,Transform end,string name,Material material,float width){var d=end.localPosition;Mesh(root,name,PrimitiveType.Capsule,d*.5f,new Vector3(width,d.magnitude*.5f,width),material,Quaternion.FromToRotation(Vector3.up,d.normalized));}
    static AnimationClip Clip(string name,bool walk)
    {
        var path=Art+"Animation/"+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}clip.frameRate=30;
        if(walk)foreach(var side in new[]{"Left","Right"}){float sign=side=="Left"?1:-1;clip.SetCurve("Hips/"+side+"UpperLeg",typeof(Transform),"localEulerAnglesRaw.x",new AnimationCurve(new Keyframe(0,0),new Keyframe(.25f,sign*25),new Keyframe(.5f,0),new Keyframe(.75f,-sign*25),new Keyframe(1,0)));clip.SetCurve("Hips/Spine/Chest/"+side+"Shoulder/"+side+"UpperArm",typeof(Transform),"localEulerAnglesRaw.x",new AnimationCurve(new Keyframe(0,0),new Keyframe(.25f,-sign*12),new Keyframe(.5f,0),new Keyframe(.75f,sign*12),new Keyframe(1,0)));}
        else clip.SetCurve("Hips/Spine",typeof(Transform),"localEulerAnglesRaw.x",new AnimationCurve(new Keyframe(0,0),new Keyframe(1,1),new Keyframe(2,0)));
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);return clip;
    }
    static AnimatorController Controller()
    {
        var path=Art+"Animation/Student.controller";var a=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);if(a!=null)return a;
        a=AnimatorController.CreateAnimatorControllerAtPath(path);a.AddParameter("Speed",AnimatorControllerParameterType.Float);var state=a.layers[0].stateMachine.AddState("Locomotion");var tree=new BlendTree{name="Idle and Walk",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,a);tree.AddChild(Clip("Idle",false),0);tree.AddChild(Clip("Walk",true),1);state.motion=tree;a.layers[0].stateMachine.defaultState=state;return a;
    }
    static AudioClip Wave(string name,float seconds,int kind)
    {
        string path=Sound+name+".wav";
        if(!File.Exists(path))using(var writer=new BinaryWriter(File.Open(path,FileMode.Create)))
        {
            const int rate=22050;int count=(int)(seconds*rate);var random=new System.Random(41+kind);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
            for(int i=0;i<count;i++){double t=(double)i/rate;double value=kind==0?.04*(Math.Sin(2*Math.PI*110*t)+.4*Math.Sin(2*Math.PI*165*t)):kind==1?Math.Exp(-45*t)*(.3*(random.NextDouble()*2-1)+.25*Math.Sin(2*Math.PI*120*t)):.22*Math.Exp(-8*t)*(Math.Sin(2*Math.PI*(kind==2?520:kind==3?660:240)*t)+.3*Math.Sin(2*Math.PI*780*t));writer.Write((short)(Math.Max(-1,Math.Min(1,value))*32760));}
        }
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    static TMP_Text Build(GameObject actor,string name,Material uniform,Transform lookTarget,AnimatorController controller,AudioClip footstep)
    {
        var old=actor.transform.Find("RiggedVisual");foreach(var renderer in actor.GetComponentsInChildren<Renderer>(true))if(old==null || !renderer.transform.IsChildOf(old))renderer.enabled=false;
        actor.transform.localScale=Vector3.one;actor.layer=2;
        foreach(var t in actor.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
        foreach(var collider in actor.GetComponentsInChildren<Collider>(true))if(collider.transform!=actor.transform && !collider.isTrigger)UnityEngine.Object.DestroyImmediate(collider);
        var capsule=actor.GetComponent<CapsuleCollider>();if(capsule!=null){capsule.height=2;capsule.center=Vector3.up;capsule.radius=.32f;}
        var cc=actor.GetComponent<CharacterController>();if(cc!=null){cc.center=Vector3.up;cc.height=2;cc.radius=.32f;}
        actor.transform.position=new Vector3(actor.transform.position.x,.05f,actor.transform.position.z);
        var model=Child(actor.transform,"RiggedVisual",Vector3.zero);model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;
        var skin=Mat("Skin",new Color(.74f,.53f,.38f));var hair=Mat("Hair",new Color(.13f,.1f,.08f));var pants=Mat("Trousers",new Color(.16f,.2f,.27f));
        var hips=Child(model.transform,"Hips",new Vector3(0,.85f,0)).transform;var spine=Child(hips,"Spine",new Vector3(0,.22f,0)).transform;var chest=Child(spine,"Chest",new Vector3(0,.22f,0)).transform;var neck=Child(chest,"Neck",new Vector3(0,.18f,0)).transform;var head=Child(neck,"Head",new Vector3(0,.18f,0)).transform;
        Mesh(chest,"UniformTorso",PrimitiveType.Capsule,new Vector3(0,-.17f,0),new Vector3(.55f,.32f,.37f),uniform,Quaternion.identity);Mesh(head,"Face",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.33f,skin,Quaternion.identity);Mesh(head,"Hair",PrimitiveType.Sphere,new Vector3(0,.085f,-.018f),new Vector3(.345f,.19f,.32f),hair,Quaternion.identity);
        foreach(int sign in new[]{-1,1})Mesh(head,"Eye_"+sign,PrimitiveType.Sphere,new Vector3(sign*.068f,.025f,.15f),Vector3.one*.035f,hair,Quaternion.identity);
        Transform upperRight=null,midRight=null,handRight=null;
        foreach(string side in new[]{"Left","Right"})
        {float sign=side=="Left"?-1:1;var shoulder=Child(chest,side+"Shoulder",new Vector3(sign*.22f,.025f,0)).transform;var upper=Child(shoulder,side+"UpperArm",new Vector3(sign*.08f,0,0)).transform;var mid=Child(upper,side+"LowerArm",new Vector3(sign*.15f,-.22f,0)).transform;var hand=Child(mid,side+"Hand",new Vector3(sign*.035f,-.23f,0)).transform;Segment(upper,mid,"Uniform"+side+"UpperArm",uniform,.14f);Segment(mid,hand,side+"Forearm",skin,.11f);Mesh(hand,"Hand",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.12f,skin,Quaternion.identity);var thigh=Child(hips,side+"UpperLeg",new Vector3(sign*.16f,-.06f,0)).transform;var shin=Child(thigh,side+"LowerLeg",new Vector3(0,-.34f,0)).transform;var foot=Child(shin,side+"Foot",new Vector3(0,-.34f,.045f)).transform;Segment(thigh,shin,side+"Thigh",pants,.2f);Segment(shin,foot,side+"Shin",pants,.18f);Mesh(foot,"Shoe",PrimitiveType.Cube,new Vector3(0,-.045f,.045f),new Vector3(.19f,.12f,.29f),hair,Quaternion.identity);if(side=="Right"){upperRight=upper;midRight=mid;handRight=hand;}}
        var animator=C<Animator>(model);animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        string avatarPath=Art+"Animation/Student_GenericAvatar.asset";var avatar=AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);if(avatar==null){avatar=AvatarBuilder.BuildGenericAvatar(model,"Hips");if(!avatar.isValid)throw new Exception("Invalid generic avatar");AssetDatabase.CreateAsset(avatar,avatarPath);}animator.avatar=avatar;
        var rig=C<Rig>(Child(model.transform,"PresentationRig",Vector3.zero));var aim=C<MultiAimConstraint>(Child(rig.transform,"HeadLook",Vector3.zero));var aimData=aim.data;aimData.constrainedObject=head;aimData.aimAxis=MultiAimConstraintData.Axis.Z;aimData.upAxis=MultiAimConstraintData.Axis.Y;aimData.worldUpType=MultiAimConstraintData.WorldUpType.SceneUp;aimData.limits=new Vector2(-45,45);aimData.maintainOffset=false;var sources=new WeightedTransformArray();sources.Add(new WeightedTransform(lookTarget,1));aimData.sourceObjects=sources;aim.data=aimData;aim.weight=.15f;
        var ik=C<TwoBoneIKConstraint>(Child(rig.transform,"HandGesture",Vector3.zero));var ikData=ik.data;ikData.root=upperRight;ikData.mid=midRight;ikData.tip=handRight;ikData.target=Child(model.transform,"HandTarget",new Vector3(.35f,1.3f,.5f)).transform;ikData.hint=Child(model.transform,"ElbowHint",new Vector3(.65f,1.1f,.1f)).transform;ikData.targetPositionWeight=1;ikData.targetRotationWeight=0;ikData.hintWeight=.5f;ik.data=ikData;ik.weight=0;
        var builder=C<RigBuilder>(model);builder.layers.Clear();builder.layers.Add(new RigLayer(rig));var motion=C<PrototypeCharacterMotion>(model);Ref(motion,"headAim",aim);Ref(motion,"handIK",ik);Ref(motion,"footstepClip",footstep);var so=new SerializedObject(motion);so.FindProperty("emitFootsteps").boolValue=actor.name=="Player";so.ApplyModifiedPropertiesWithoutUndo();
        var labelObject=Child(actor.transform,"CharacterName",new Vector3(0,2.12f,0));labelObject.transform.localScale=Vector3.one*.14f;var label=C<TextMeshPro>(labelObject);label.font=TMP_Settings.defaultFontAsset;label.text=name;label.fontSize=10;label.alignment=TextAlignmentOptions.Center;label.color=new Color(.1f,.18f,.20f);label.rectTransform.sizeDelta=new Vector2(16,4);label.GetComponent<Renderer>().enabled=true;C<WorldCharacterLabel>(labelObject);return label;
    }
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");Folder(Art+"Animation");Folder(Sound.TrimEnd('/'));
        var controller=Controller();var ambience=Wave("CourtyardAmbience",12,0);var footstep=Wave("SoftFootstep",.14f,1);var complete=Wave("ObjectiveComplete",.45f,2);var success=Wave("QTESuccess",.45f,3);var retry=Wave("QTERetry",.3f,4);
        var player=GameObject.Find("Player");var target=player.transform.Find("CameraTarget");target.localPosition=new Vector3(0,1.45f,0);var leader=GameObject.Find("_Level/BullyGroup/Bully_Leader");var argaUniform=Mat("UniformArga",new Color(.19f,.42f,.65f));var label=Build(player,"Arga",argaUniform,leader.transform,controller,footstep);var pov=UnityEngine.Object.FindAnyObjectByType<PlayablePOVController>();Ref(pov,"characterLabel",label);var so=new SerializedObject(pov);var list=so.FindProperty("uniformRenderers");var renderers=player.transform.Find("RiggedVisual").GetComponentsInChildren<Renderer>().Where(r=>r.name.StartsWith("Uniform")).ToArray();list.arraySize=renderers.Length;for(int i=0;i<renderers.Length;i++)list.GetArrayElementAtIndex(i).objectReferenceValue=renderers[i];so.ApplyModifiedPropertiesWithoutUndo();
        Build(GameObject.Find("_Level/StoryCharacters/Arga"),"Arga",argaUniform,target,controller,footstep);Build(GameObject.Find("_Level/StoryCharacters/Dimas"),"Dimas",Mat("UniformDimas",new Color(.78f,.58f,.20f)),target,controller,footstep);Build(GameObject.Find("_Level/StoryCharacters/Nara"),"Nara",Mat("UniformNara",new Color(.23f,.56f,.43f)),target,controller,footstep);Build(GameObject.Find("_Level/BuNadia"),"Bu Nadia",Mat("UniformTeacher",new Color(.48f,.39f,.60f)),target,controller,footstep);Build(leader,"Raka",Mat("UniformRaka",new Color(.64f,.30f,.23f)),target,controller,footstep);
        var friends=GameObject.Find("_Level/BullyGroup").GetComponentsInChildren<BullyingGame.NPC.BullyNPC>().Where(b=>b.gameObject!=leader).ToArray();for(int i=0;i<friends.Length;i++)Build(friends[i].gameObject,i==0?"Bima":"Fajar",Mat("UniformMember",new Color(.45f,.40f,.32f)),target,controller,footstep);
        var audio=C<PrototypeAudioDirector>(Child(GameObject.Find("[GameSystem]").transform,"PrototypeAudio",Vector3.zero));Ref(audio,"ambience",ambience);Ref(audio,"objectiveComplete",complete);Ref(audio,"qteSuccess",success);Ref(audio,"qteRetry",retry);
        var surface=GameObject.Find("_Level/Environtment").GetComponent<Unity.AI.Navigation.NavMeshSurface>();surface.layerMask=~(1<<2);surface.BuildNavMesh();if(surface.navMeshData!=null && !EditorUtility.IsPersistent(surface.navMeshData))AssetDatabase.CreateAsset(surface.navMeshData,AssetDatabase.GenerateUniqueAssetPath(Art+"NavMesh_School.asset"));
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(player.scene);if(!EditorSceneManager.SaveScene(player.scene))throw new Exception("Scene save failed");return "Saved 8 original prototype characters with Generic locomotion, head aim and hand IK; 5 original audio clips; grounded collision and character-excluded navigation bake.";
    }
}

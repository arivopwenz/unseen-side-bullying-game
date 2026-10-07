using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class FinalEditorSetup
{
    public static string Level()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Levels/Level_01/Level01.unity");
        AssetDatabase.ImportAsset("Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader",ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        return "Saved Level01 loaded and revised TMP shader imported.";
    }
    public static string Bootstrap()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode required");
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity");
        return "Bootstrap opened in existing Editor; ready for Play > Mulai baru.";
    }
}

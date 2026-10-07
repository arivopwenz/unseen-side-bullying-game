using System;
using UnityEngine;
using UnityEditor;
public static class EditorRecovery
{
    public static string Run()
    {
        if(Application.isPlaying) throw new Exception("Stop Play Mode before compiling.");
        var property=typeof(AssetDatabase).GetProperty("DesiredWorkerCount");
        if(property!=null && property.CanWrite) property.SetValue(null,1);
        var force=typeof(AssetDatabase).GetMethod("ForceToDesiredWorkerCount",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public);
        force?.Invoke(null,null);
        AssetDatabase.Refresh();
        GC.Collect();
        return "Using existing Editor only. Import worker limit: "+(property!=null ? property.GetValue(null).ToString() : "API unavailable")+". Source refresh requested.";
    }
}

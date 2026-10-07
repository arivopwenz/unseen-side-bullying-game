using UnityEditor;
public static class EditorTestOptions
{
    public static string Set()
    {
        const string prefix="BullyingGame.Automation.";
        if(!EditorPrefs.HasKey(prefix+"PlayModeOptionsEnabled"))
        {
            EditorPrefs.SetBool(prefix+"PlayModeOptionsEnabled",EditorSettings.enterPlayModeOptionsEnabled);
            EditorPrefs.SetInt(prefix+"PlayModeOptions",(int)EditorSettings.enterPlayModeOptions);
        }
        EditorSettings.enterPlayModeOptionsEnabled=true;
        EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        AssetDatabase.DesiredWorkerCount=1;AssetDatabase.ForceToDesiredWorkerCount();
        return "Temporary automation play mode options set; original values retained.";
    }
    public static string Restore()
    {
        const string prefix="BullyingGame.Automation.";
        if(EditorPrefs.HasKey(prefix+"PlayModeOptionsEnabled"))
        {
            EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)EditorPrefs.GetInt(prefix+"PlayModeOptions");
            EditorSettings.enterPlayModeOptionsEnabled=EditorPrefs.GetBool(prefix+"PlayModeOptionsEnabled");
            EditorPrefs.DeleteKey(prefix+"PlayModeOptions");EditorPrefs.DeleteKey(prefix+"PlayModeOptionsEnabled");
        }
        return "Original Editor Play Mode options restored.";
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using BullyingGame.UI;

public static class Chapter12MenuCheck
{
    public static async Task<string> Run()
    {
        if (!Application.isPlaying) throw new Exception("Play Mode required");
        MainMenuFlow menu = null;
        double deadline = EditorApplication.timeSinceStartup + 15;
        while (menu == null && EditorApplication.timeSinceStartup < deadline)
        {
            menu = UnityEngine.Object.FindAnyObjectByType<MainMenuFlow>();
            await Task.Delay(20);
        }
        if (menu == null) throw new Exception("Menu missing");
        var serialized = new SerializedObject(menu);
        var panel = (GameObject)serialized.FindProperty("settingsPanel").objectReferenceValue;
        var open = (Button)serialized.FindProperty("settingsButton").objectReferenceValue;
        open.onClick.Invoke();
        await Task.Delay(300);
        if (!panel.activeInHierarchy) throw new Exception("Settings not open");
        if (panel.GetComponentsInChildren<Toggle>().Length != 3) throw new Exception("Three settings required");
        var canvas = panel.GetComponentInParent<Canvas>();
        var rect = panel.GetComponent<RectTransform>();
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        foreach (var corner in corners)
        {
            var point = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, corner);
            if (point.x < 0 || point.y < 0 || point.x > Screen.width || point.y > Screen.height)
                throw new Exception("Settings outside screen: " + point);
        }
        string path = Path.GetFullPath("Logs/AutomationChapter12/MenuSettings.png");
        if (File.Exists(path)) File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);
        deadline = EditorApplication.timeSinceStartup + 5;
        while (!File.Exists(path) && EditorApplication.timeSinceStartup < deadline) await Task.Delay(20);
        if (!File.Exists(path)) throw new Exception("Screenshot missing");
        var close = panel.transform.Find("ClosePreferences").GetComponent<Button>();
        if (close.onClick.GetPersistentEventCount() != 1) throw new Exception("Close binding missing");
        close.onClick.Invoke();
        if (panel.activeSelf) throw new Exception("Actual close button failed");
        await Task.Delay(100);
        return "PASS menu layout: 9 checks; open, three toggles, four corners on screen, capture, persistent close and actual close. Preferences and player save untouched.";
    }
}

using UnityEngine;
using UnityEditor;

// Pastikan file ini berada di dalam folder bernama "Editor"
[CustomEditor(typeof(LimitLocationConstraint))]
[CanEditMultipleObjects]
public class LimitLocationConstraintEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Mendeteksi perubahan agar Undo/Redo bekerja
        serializedObject.Update();
        LimitLocationConstraint script = (LimitLocationConstraint)target;

        // Tampilan Setting ala Blender
        EditorGUILayout.BeginVertical("helpbox");
        
        // Perbaikan: Menggunakan baris horizontal agar lebih rapi
        EditorGUILayout.BeginHorizontal();
        script.limitX = EditorGUILayout.ToggleLeft("Limit X", script.limitX, GUILayout.Width(80));
        if(script.limitX) {
            script.minX = EditorGUILayout.FloatField(script.minX);
            script.maxX = EditorGUILayout.FloatField(script.maxX);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        script.limitY = EditorGUILayout.ToggleLeft("Limit Y", script.limitY, GUILayout.Width(80));
        if(script.limitY) {
            script.minY = EditorGUILayout.FloatField(script.minY);
            script.maxY = EditorGUILayout.FloatField(script.maxY);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        script.affectTransform = EditorGUILayout.ToggleLeft("Affect Transform", script.affectTransform);
        script.owner = (LimitLocationConstraint.SpaceOwner)EditorGUILayout.EnumPopup("Owner", script.owner);
        script.influence = EditorGUILayout.Slider("Influence", script.influence, 0f, 1f);
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Visual Joystick Control", EditorStyles.boldLabel);

        // Area Kotak Visual
        Rect boxRect = GUILayoutUtility.GetRect(160, 160, GUILayout.ExpandWidth(false));
        EditorGUI.DrawRect(boxRect, new Color(0.15f, 0.15f, 0.15f));

        // Crosshair
        float centerX = boxRect.x + boxRect.width / 2;
        float centerY = boxRect.y + boxRect.height / 2;
        EditorGUI.DrawRect(new Rect(centerX, boxRect.y, 1, 160), new Color(0.3f, 0.3f, 0.3f));
        EditorGUI.DrawRect(new Rect(boxRect.x, centerY, 160, 1), new Color(0.3f, 0.3f, 0.3f));

        // Interaksi Mouse
        Event e = Event.current;
        if (boxRect.Contains(e.mousePosition) && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown))
        {
            Undo.RecordObject(script, "Move Limit Joystick");
            
            float rawX = (e.mousePosition.x - boxRect.x) / boxRect.width;
            float rawY = (e.mousePosition.y - boxRect.y) / boxRect.height;

            // Mapping posisi mouse ke range Min/Max
            script.joystickPos.x = Mathf.Lerp(script.minX, script.maxX, rawX);
            script.joystickPos.y = Mathf.Lerp(script.maxY, script.minY, rawY);

            // Update posisi objek secara instan
            if (script.affectTransform)
            {
                if (script.owner == LimitLocationConstraint.SpaceOwner.Local)
                    script.transform.localPosition = new Vector3(script.joystickPos.x, script.joystickPos.y, script.transform.localPosition.z);
                else
                    script.transform.position = new Vector3(script.joystickPos.x, script.joystickPos.y, script.transform.position.z);
            }

            EditorUtility.SetDirty(target);
            e.Use();
        }

        // Gambar Handle (Titik Kuning)
        float visualX = Mathf.InverseLerp(script.minX, script.maxX, script.joystickPos.x);
        float visualY = Mathf.InverseLerp(script.minY, script.maxY, script.joystickPos.y);

        Vector2 handlePos = new Vector2(
            boxRect.x + visualX * boxRect.width,
            boxRect.y + (1 - visualY) * boxRect.height
        );
        EditorGUI.DrawRect(new Rect(handlePos.x - 5, handlePos.y - 5, 10, 10), new Color(1f, 0.8f, 0.2f));

        // Terapkan perubahan ke serialized object
        serializedObject.ApplyModifiedProperties();
        
        if (e.type == EventType.MouseDrag)
        {
            Repaint();
            SceneView.RepaintAll();
        }
    }
}
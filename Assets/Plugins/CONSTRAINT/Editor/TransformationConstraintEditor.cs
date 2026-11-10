using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransformationConstraint))]
public class TransformationConstraintEditor : Editor
{
    private string[] tabLabels = { "Location", "Rotation", "Scale" };

    public override void OnInspectorGUI()
    {
        TransformationConstraint script = (TransformationConstraint)target;
        
        // --- Bagian Atas (Target & Influence) tetap sama ---
        DrawDefaultHeader(script);

        EditorGUILayout.Space(10);

        // --- MAP FROM ---
        EditorGUILayout.LabelField("▼ Map From", EditorStyles.boldLabel);
        script.mapFromType = (TransformationConstraint.MapType)GUILayout.Toolbar((int)script.mapFromType, tabLabels, EditorStyles.toolbarButton);
        
        // Ambil data settings berdasarkan tab Map From yang dipilih
        var fromSet = script.GetSettings(script.mapFromType);
        
        EditorGUILayout.BeginVertical("box");
        fromSet.fromMin = EditorGUILayout.Vector3Field("Min Range", fromSet.fromMin);
        fromSet.fromMax = EditorGUILayout.Vector3Field("Max Range", fromSet.fromMax);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- MAP TO ---
        EditorGUILayout.LabelField("▼ Map To", EditorStyles.boldLabel);
        script.mapToType = (TransformationConstraint.MapType)GUILayout.Toolbar((int)script.mapToType, tabLabels, EditorStyles.toolbarButton);
        
        // Ambil data settings berdasarkan tab Map To yang dipilih
        var toSet = script.GetSettings(script.mapToType);
        
        EditorGUILayout.BeginVertical("box");
        DrawAxisMapping("X", toSet.toX);
        EditorGUILayout.Space(5);
        DrawAxisMapping("Y", toSet.toY);
        EditorGUILayout.Space(5);
        DrawAxisMapping("Z", toSet.toZ);
        EditorGUILayout.EndVertical();

        if (GUI.changed) { EditorUtility.SetDirty(target); }
    }

    void DrawDefaultHeader(TransformationConstraint script)
    {
        EditorGUILayout.BeginVertical("helpbox");
        script.targetObject = (Transform)EditorGUILayout.ObjectField("Target", script.targetObject, typeof(Transform), true);
        script.sourceObject = (Transform)EditorGUILayout.ObjectField("Bone", script.sourceObject, typeof(Transform), true);
        script.extrapolate = EditorGUILayout.Toggle("Extrapolate", script.extrapolate);
        script.targetSpace = (TransformationConstraint.SpaceOwner)EditorGUILayout.EnumPopup("Target Space", script.targetSpace);
        script.ownerSpace = (TransformationConstraint.SpaceOwner)EditorGUILayout.EnumPopup("Owner Space", script.ownerSpace);
        script.influence = EditorGUILayout.Slider("Influence", script.influence, 0f, 1f);
        EditorGUILayout.EndVertical();
    }

    void DrawAxisMapping(string label, TransformationConstraint.MapData data)
    {
        EditorGUILayout.LabelField(label + " Axis Target Settings", EditorStyles.miniBoldLabel);
        data.sourceAxis = (TransformationConstraint.AxisSource)EditorGUILayout.EnumPopup(label + " Source Axis", data.sourceAxis);
        data.min = EditorGUILayout.FloatField("  Min", data.min);
        data.max = EditorGUILayout.FloatField("  Max", data.max);
    }
}
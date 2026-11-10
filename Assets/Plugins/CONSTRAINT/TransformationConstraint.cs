using UnityEngine;
using System;

[ExecuteAlways]
public class TransformationConstraint : MonoBehaviour
{
    public enum MapType { Location, Rotation, Scale }
    public enum SpaceOwner { World, Local }
    public enum AxisSource { X, Y, Z }

    [Serializable]
    public class MapData
    {
        public AxisSource sourceAxis = AxisSource.X;
        public float min;
        public float max;
        public MapData(AxisSource axis, float dMin, float dMax) { sourceAxis = axis; min = dMin; max = dMax; }
    }

    [Serializable]
    public class MappingSettings
    {
        public Vector3 fromMin = new Vector3(-1, -1, -1);
        public Vector3 fromMax = new Vector3(1, 1, 1);
        public MapData toX = new MapData(AxisSource.X, 0, 0);
        public MapData toY = new MapData(AxisSource.Y, 0, 0);
        public MapData toZ = new MapData(AxisSource.Z, 0, 0);
    }

    public Transform targetObject;
    public Transform sourceObject;
    public bool extrapolate = true;
    public SpaceOwner targetSpace = SpaceOwner.Local;
    public SpaceOwner ownerSpace = SpaceOwner.Local;
    [Range(0f, 1f)] public float influence = 1f;

    public MapType mapFromType = MapType.Location;
    public MapType mapToType = MapType.Location;

    public MappingSettings locationSettings = new MappingSettings();
    public MappingSettings rotationSettings = new MappingSettings();
    public MappingSettings scaleSettings = new MappingSettings();

    // --- VARIABEL KUNCI: Untuk menyimpan posisi asli roda ---
    [SerializeField, HideInInspector] private Vector3 initialPosition;
    [SerializeField, HideInInspector] private Vector3 initialRotation;
    [SerializeField, HideInInspector] private Vector3 initialScale;
    private bool hasCapturedInitial = false;

    // Tombol untuk mengambil posisi awal secara manual jika roda dipindahkan
    [ContextMenu("Capture Current Transform as Initial")]
    public void CaptureInitialValues()
    {
        if (targetObject == null) return;
        initialPosition = targetObject.localPosition;
        initialRotation = targetObject.localEulerAngles;
        initialScale = targetObject.localScale;
        hasCapturedInitial = true;
    }

    void LateUpdate()
    {
        if (targetObject == null || sourceObject == null || influence <= 0) return;

        // Otomatis capture jika belum punya data awal
        if (!hasCapturedInitial) CaptureInitialValues();

        MappingSettings fromData = GetSettings(mapFromType);
        MappingSettings toData = GetSettings(mapToType);

        Vector3 sourceVal = GetSourceValue(sourceObject, mapFromType);

        float resX = MapAxis(sourceVal, toData.toX, fromData.fromMin, fromData.fromMax);
        float resY = MapAxis(sourceVal, toData.toY, fromData.fromMin, fromData.fromMax);
        float resZ = MapAxis(sourceVal, toData.toZ, fromData.fromMin, fromData.fromMax);

        ApplyToTarget(new Vector3(resX, resY, resZ));
    }

    public MappingSettings GetSettings(MapType type)
    {
        if (type == MapType.Location) return locationSettings;
        if (type == MapType.Rotation) return rotationSettings;
        return scaleSettings;
    }

    float MapAxis(Vector3 sourceVector, MapData data, Vector3 fMin, Vector3 fMax)
    {
        float val = (data.sourceAxis == AxisSource.X) ? sourceVector.x : (data.sourceAxis == AxisSource.Y) ? sourceVector.y : sourceVector.z;
        float inMin = (data.sourceAxis == AxisSource.X) ? fMin.x : (data.sourceAxis == AxisSource.Y) ? fMin.y : fMin.z;
        float inMax = (data.sourceAxis == AxisSource.X) ? fMax.x : (data.sourceAxis == AxisSource.Y) ? fMax.y : fMax.z;

        if (Mathf.Approximately(inMin, inMax)) return data.min;

        float t = (val - inMin) / (inMax - inMin);
        if (!extrapolate) t = Mathf.Clamp01(t);

        return Mathf.LerpUnclamped(data.min, data.max, t);
    }

    Vector3 GetSourceValue(Transform s, MapType type)
    {
        if (type == MapType.Location) return (ownerSpace == SpaceOwner.Local) ? s.localPosition : s.position;
        if (type == MapType.Rotation) return (ownerSpace == SpaceOwner.Local) ? s.localEulerAngles : s.eulerAngles;
        return s.localScale;
    }

    void ApplyToTarget(Vector3 res)
    {
        if (mapToType == MapType.Location)
        {
            // Tambahkan hasil mapping ke posisi awal (Additive)
            Vector3 finalPos = initialPosition + res;
            if (targetSpace == SpaceOwner.Local) targetObject.localPosition = Vector3.Lerp(targetObject.localPosition, finalPos, influence);
            else targetObject.position = Vector3.Lerp(targetObject.position, finalPos, influence);
        }
        else if (mapToType == MapType.Rotation)
        {
            // Tambahkan hasil mapping ke rotasi awal
            Quaternion targetRot = Quaternion.Euler(initialRotation + res);
            if (targetSpace == SpaceOwner.Local) targetObject.localRotation = Quaternion.Slerp(targetObject.localRotation, targetRot, influence);
            else targetObject.rotation = Quaternion.Slerp(targetObject.rotation, targetRot, influence);
        }
        else
        {
            targetObject.localScale = Vector3.Lerp(targetObject.localScale, initialScale + res, influence);
        }
    }
}
using UnityEngine;
using System.Linq;

/// <summary>Full visuals are enabled by default. Lightweight mode hides only very dense mesh renderers.</summary>
[ExecuteAlways]
public sealed class AreaSekolahGraphicsController : MonoBehaviour
{
    [SerializeField] private bool lightweightMode;
    [SerializeField, Min(10000)] private int triangleThreshold = 500000;
    [SerializeField] private Renderer[] affectedRenderers;

    public bool LightweightMode => lightweightMode;

    private void OnEnable() => ApplyGraphicsMode();
    private void OnValidate() => ApplyGraphicsMode();

    [ContextMenu("Refresh Heavy Mesh List")]
    public void RefreshHeavyMeshList()
    {
        affectedRenderers = GetComponentsInChildren<MeshFilter>(true)
            .Where(filter => filter.sharedMesh != null &&
                filter.sharedMesh.triangles.Length / 3 >= triangleThreshold)
            .Select(filter => filter.GetComponent<Renderer>())
            .Where(renderer => renderer != null)
            .Distinct()
            .ToArray();
        ApplyGraphicsMode();
    }

    [ContextMenu("Use Full Graphics")]
    public void UseFullGraphics()
    {
        lightweightMode = false;
        ApplyGraphicsMode();
    }

    [ContextMenu("Use Lightweight Graphics")]
    public void UseLightweightGraphics()
    {
        lightweightMode = true;
        RefreshHeavyMeshList();
    }

    public void SetLightweightMode(bool enabled)
    {
        lightweightMode = enabled;
        if (affectedRenderers == null || affectedRenderers.Length == 0)
            RefreshHeavyMeshList();
        ApplyGraphicsMode();
    }

    private void ApplyGraphicsMode()
    {
        if (affectedRenderers == null) return;
        foreach (var item in affectedRenderers)
            if (item != null) item.enabled = !lightweightMode;
    }
}
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls full-color graphics features for AreaSekolah (post-processing volume, camera AA/PP,
/// lighting, reflection probes, fog, skybox, and optional heavy-mesh hiding in lightweight mode).
/// </summary>
[ExecuteAlways]
public sealed class AreaSekolahGraphicsController : MonoBehaviour
{
    private static readonly Color FullColorSunColor = new Color(1f, 0.96f, 0.88f, 1f);
    private static readonly Color FullColorFogColor = new Color(0.76f, 0.86f, 0.96f, 1f);
    private static readonly Color FullColorAmbientSky = new Color(0.68f, 0.78f, 0.92f, 1f);
    private static readonly Color FullColorAmbientEquator = new Color(0.58f, 0.6f, 0.64f, 1f);
    private static readonly Color FullColorAmbientGround = new Color(0.42f, 0.4f, 0.37f, 1f);

    private static readonly Color LightweightAmbientSky = new Color(0.54f, 0.66f, 0.75f, 1f);
    private static readonly Color LightweightAmbientEquator = new Color(0.5f, 0.52f, 0.5f, 1f);
    private static readonly Color LightweightAmbientGround = new Color(0.24f, 0.27f, 0.3f, 1f);

    [Header("Full Color Graphics Toggle")]
    [Tooltip("Enable vibrant full-color graphics (Post-Processing Volume, Skybox, Fog, Ambient, Reflection Probes, Soft Shadows, Camera PP/SMAA).")]
    [SerializeField] private bool enableFullColorGraphics = true;

    [Header("Visual Targets (Auto-resolved if unassigned)")]
    [SerializeField] private Volume globalVolume = null;
    [SerializeField] private Light mainDirectionalLight = null;
    [SerializeField] private Material fullColorSkybox = null;
    [SerializeField] private Material lightweightSkybox = null;

    [Header("Feature Scope")]
    [SerializeField] private bool controlSceneRenderSettings = true;
    [SerializeField] private bool controlSceneCameras = true;

    [Header("Lightweight Mesh Options (Legacy Compatibility)")]
    [SerializeField] private bool lightweightMode = false;
    [SerializeField] private bool hideHeavyMeshesInLightweightMode = false;
    [SerializeField, Min(10000)] private int triangleThreshold = 500000;
    [SerializeField] private Renderer[] affectedRenderers = null;

    [SerializeField, HideInInspector] private bool lastEnableFullColorGraphics = true;
    [SerializeField, HideInInspector] private bool lastLightweightMode = false;

    public bool FullColorGraphicsEnabled => enableFullColorGraphics;
    public bool LightweightMode => lightweightMode;

    private void OnEnable()
    {
        SyncToggleState();
        ApplyGraphicsMode();
    }

    private void Start() => ApplyGraphicsMode();

    private void OnValidate()
    {
        SyncToggleState();
        ApplyGraphicsMode();
    }

    [ContextMenu("Enable Full Color Graphics")]
    public void EnableFullColorGraphics() => SetFullColorGraphics(true);

    [ContextMenu("Disable Full Color Graphics")]
    public void DisableFullColorGraphics() => SetFullColorGraphics(false);

    [ContextMenu("Use Full Graphics")]
    public void UseFullGraphics() => SetFullColorGraphics(true);

    [ContextMenu("Use Lightweight Graphics")]
    public void UseLightweightGraphics()
    {
        SetFullColorGraphics(false);
        if (hideHeavyMeshesInLightweightMode)
            RefreshHeavyMeshList();
    }

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
        ApplyHeavyMeshRenderers();
    }

    /// <summary>Enables or disables full-color graphics and updates all linked scene visuals.</summary>
    public void SetFullColorGraphics(bool enabled)
    {
        enableFullColorGraphics = enabled;
        lightweightMode = !enabled;
        lastEnableFullColorGraphics = enableFullColorGraphics;
        lastLightweightMode = lightweightMode;
        ApplyGraphicsMode();
    }

    /// <summary>Legacy entry point kept for callers expecting lightweightMode toggle.</summary>
    public void SetLightweightMode(bool enabled)
    {
        SetFullColorGraphics(!enabled);
        if (enabled && hideHeavyMeshesInLightweightMode && (affectedRenderers == null || affectedRenderers.Length == 0))
            RefreshHeavyMeshList();
    }

    private void SyncToggleState()
    {
        if (enableFullColorGraphics != lastEnableFullColorGraphics)
        {
            lightweightMode = !enableFullColorGraphics;
        }
        else if (lightweightMode != lastLightweightMode)
        {
            enableFullColorGraphics = !lightweightMode;
        }

        lastEnableFullColorGraphics = enableFullColorGraphics;
        lastLightweightMode = lightweightMode;
    }

    public void ApplyGraphicsMode()
    {
        ResolveReferencesIfNeeded();
        ApplyVolumeSettings();
        ApplyReflectionProbes();
        ApplyDirectionalLightSettings();
        ApplyRenderSettings();
        ApplyCameraSettings();
        ApplyHeavyMeshRenderers();
    }

    private void ResolveReferencesIfNeeded()
    {
        if (globalVolume == null)
            globalVolume = GetComponentInChildren<Volume>(true);

        if (mainDirectionalLight == null)
        {
            mainDirectionalLight = GetComponentsInChildren<Light>(true)
                .FirstOrDefault(l => l != null && l.type == LightType.Directional);
        }
    }

    private void ApplyVolumeSettings()
    {
        if (globalVolume == null) return;

        globalVolume.enabled = enableFullColorGraphics;
        globalVolume.weight = enableFullColorGraphics ? 1f : 0f;
    }

    private void ApplyReflectionProbes()
    {
        var probes = GetComponentsInChildren<ReflectionProbe>(true);
        foreach (var probe in probes)
        {
            if (probe != null)
                probe.enabled = enableFullColorGraphics;
        }
    }

    private void ApplyDirectionalLightSettings()
    {
        if (mainDirectionalLight == null) return;

        if (enableFullColorGraphics)
        {
            mainDirectionalLight.color = FullColorSunColor;
            mainDirectionalLight.intensity = 1.35f;
            mainDirectionalLight.shadows = LightShadows.Soft;
            mainDirectionalLight.shadowStrength = 0.82f;
        }
        else
        {
            mainDirectionalLight.color = Color.white;
            mainDirectionalLight.intensity = 1.0f;
            mainDirectionalLight.shadows = LightShadows.Hard;
            mainDirectionalLight.shadowStrength = 0.5f;
        }
    }

    private void ApplyRenderSettings()
    {
        if (!controlSceneRenderSettings) return;

        if (enableFullColorGraphics)
        {
            if (fullColorSkybox != null)
                RenderSettings.skybox = fullColorSkybox;

            RenderSettings.fog = true;
            RenderSettings.fogColor = FullColorFogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 95f;
            RenderSettings.fogEndDistance = 320f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = FullColorAmbientSky;
            RenderSettings.ambientEquatorColor = FullColorAmbientEquator;
            RenderSettings.ambientGroundColor = FullColorAmbientGround;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 1f;
        }
        else
        {
            if (lightweightSkybox != null)
                RenderSettings.skybox = lightweightSkybox;

            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = LightweightAmbientSky;
            RenderSettings.ambientEquatorColor = LightweightAmbientEquator;
            RenderSettings.ambientGroundColor = LightweightAmbientGround;
            RenderSettings.ambientIntensity = 0.85f;
            RenderSettings.reflectionIntensity = 0.3f;
        }
    }

    private void ApplyCameraSettings()
    {
        if (!controlSceneCameras) return;

        var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
        foreach (var cam in cameras)
        {
            if (cam == null || cam.cameraType != CameraType.Game) continue;

            var additionalData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (additionalData == null) continue;

            additionalData.renderPostProcessing = enableFullColorGraphics;
            additionalData.antialiasing = enableFullColorGraphics
                ? AntialiasingMode.SubpixelMorphologicalAntiAliasing
                : AntialiasingMode.None;
            additionalData.antialiasingQuality = AntialiasingQuality.High;
        }
    }

    private void ApplyHeavyMeshRenderers()
    {
        if (affectedRenderers == null) return;

        bool renderHeavyMeshes = enableFullColorGraphics || !hideHeavyMeshesInLightweightMode;
        foreach (var item in affectedRenderers)
        {
            if (item != null)
                item.enabled = renderHeavyMeshes;
        }
    }
}

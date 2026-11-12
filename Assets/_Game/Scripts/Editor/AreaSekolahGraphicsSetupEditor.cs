using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace BullyingGame.EditorTools
{
    /// <summary>
    /// Repairs and restores full-color graphics assets (VolumeProfile sub-assets, URP PC asset,
    /// AreaSekolah prefab controller wiring, and scene lighting/post-processing settings).
    /// </summary>
    public static class AreaSekolahGraphicsSetupEditor
    {
        private const string VolumeProfilePath = "Assets/_Game/Scenes/Levels/env_design/AreaSekolah_VolumeProfile.asset";
        private const string AreaSekolahPrefabPath = "Assets/_Game/Prefabs/Environment/AreaSekolah.prefab";
        private const string AreaSekolahScenePath = "Assets/_Game/Scenes/Levels/env_design/area-sekolah.unity";
        private const string Level02ScenePath = "Assets/_Game/Scenes/Levels/Level_02/Level02.unity";
        private const string SkyboxGuid = "12ccb828343c3405fb94289774b51aad";

        [MenuItem("Tools/The Unseen Side/Restore AreaSekolah Full Color Graphics")]
        public static void RestoreAllInteractive()
        {
            RestoreAllInternal();
            Debug.Log("[AreaSekolahGraphicsSetup] Full-color graphics settings restored successfully.");
        }

        [InitializeOnLoadMethod]
        private static void RepairBrokenVolumeProfileOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;

                var skyboxMat = LoadSkyboxMaterial();
                var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
                bool repairedAny = false;

                if (profile != null && (profile.components == null || profile.components.Count == 0 || profile.components.Exists(c => c == null)))
                {
                    profile = RepairVolumeProfile();
                    repairedAny = true;
                    Debug.Log("[AreaSekolahGraphicsSetup] Auto-repaired broken AreaSekolah_VolumeProfile sub-assets.");
                }

                // Wire missing prefab references once without overwriting user toggle state.
                if (File.Exists(AreaSekolahPrefabPath))
                {
                    var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(AreaSekolahPrefabPath);
                    var prefabCtrl = prefabAsset != null ? prefabAsset.GetComponent<AreaSekolahGraphicsController>() : null;
                    if (prefabCtrl != null && NeedsControllerReferenceRepair(prefabCtrl))
                    {
                        RepairAreaSekolahPrefab(profile, skyboxMat, forceEnableFullColor: false);
                        repairedAny = true;
                    }
                }

                foreach (var controller in Object.FindObjectsByType<AreaSekolahGraphicsController>(FindObjectsInactive.Include))
                {
                    if (controller == null) continue;
                    if (WireControllerReferences(controller, profile, skyboxMat))
                    {
                        EditorUtility.SetDirty(controller);
                        repairedAny = true;
                    }
                    if (repairedAny || controller.FullColorGraphicsEnabled)
                        controller.ApplyGraphicsMode();
                }
            };
        }

        /// <summary>Batchmode entry point for restoring all full-color graphics assets and scenes.</summary>
        public static void RestoreAllFromBatch()
        {
            RestoreAllInternal();
            Debug.Log("[AreaSekolahGraphicsSetup] Batchmode restore completed cleanly.");
        }

        private static void RestoreAllInternal()
        {
            EnvironmentOptimizer.ApplyFullColorURPSettings();
            var profile = RepairVolumeProfile();
            var skyboxMat = LoadSkyboxMaterial();

            RepairAreaSekolahPrefab(profile, skyboxMat);
            RepairScene(AreaSekolahScenePath, skyboxMat);
            RepairScene(Level02ScenePath, skyboxMat);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material LoadSkyboxMaterial()
        {
            var path = AssetDatabase.GUIDToAssetPath(SkyboxGuid);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        private static VolumeProfile RepairVolumeProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
            {
                Debug.LogError($"[AreaSekolahGraphicsSetup] Missing VolumeProfile at {VolumeProfilePath}");
                return null;
            }

            // Clear broken null references and recreate properly serialized sub-assets.
            profile.components.RemoveAll(c => c == null);
            foreach (var existing in AssetDatabase.LoadAllAssetsAtPath(VolumeProfilePath))
            {
                if (existing != null && existing != profile && AssetDatabase.IsSubAsset(existing))
                    Object.DestroyImmediate(existing, true);
            }
            profile.components.Clear();

            var tonemapping = AddSubAssetComponent<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            var colorAdjustments = AddSubAssetComponent<ColorAdjustments>(profile);
            colorAdjustments.postExposure.Override(0.15f);
            colorAdjustments.contrast.Override(12f);
            colorAdjustments.saturation.Override(12f);

            var bloom = AddSubAssetComponent<Bloom>(profile);
            bloom.intensity.Override(0.25f);
            bloom.threshold.Override(1.0f);
            bloom.scatter.Override(0.5f);

            var vignette = AddSubAssetComponent<Vignette>(profile);
            vignette.intensity.Override(0.15f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static T AddSubAssetComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var component = profile.Add<T>(false);
            component.name = typeof(T).Name;
            component.active = true;
            component.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            if (!AssetDatabase.IsSubAsset(component))
                AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        private static void RepairAreaSekolahPrefab(VolumeProfile profile, Material skyboxMat, bool forceEnableFullColor = true)
        {
            if (!File.Exists(AreaSekolahPrefabPath))
            {
                Debug.LogError($"[AreaSekolahGraphicsSetup] Prefab not found at {AreaSekolahPrefabPath}");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(AreaSekolahPrefabPath);
            try
            {
                var controller = root.GetComponent<AreaSekolahGraphicsController>();
                if (controller == null)
                    controller = root.AddComponent<AreaSekolahGraphicsController>();

                WireControllerReferences(controller, profile, skyboxMat);

                var grassGround = FindTransformByName(root.transform, "Ground_Rumput_Utama");
                if (grassGround != null)
                    grassGround.gameObject.SetActive(true);

                if (forceEnableFullColor)
                {
                    var so = new SerializedObject(controller);
                    SetBoolProperty(so, "enableFullColorGraphics", true);
                    SetBoolProperty(so, "lightweightMode", false);
                    SetBoolProperty(so, "lastEnableFullColorGraphics", true);
                    SetBoolProperty(so, "lastLightweightMode", false);
                    SetBoolProperty(so, "controlSceneRenderSettings", true);
                    SetBoolProperty(so, "controlSceneCameras", true);
                    SetBoolProperty(so, "hideHeavyMeshesInLightweightMode", false);
                    so.ApplyModifiedPropertiesWithoutUndo();
                    controller.SetFullColorGraphics(true);
                }
                else
                {
                    controller.ApplyGraphicsMode();
                }

                PrefabUtility.SaveAsPrefabAsset(root, AreaSekolahPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool NeedsControllerReferenceRepair(AreaSekolahGraphicsController controller)
        {
            var so = new SerializedObject(controller);
            var volumeProp = so.FindProperty("globalVolume");
            var lightProp = so.FindProperty("mainDirectionalLight");
            var skyboxProp = so.FindProperty("fullColorSkybox");
            return (volumeProp != null && volumeProp.objectReferenceValue == null)
                || (lightProp != null && lightProp.objectReferenceValue == null)
                || (skyboxProp != null && skyboxProp.objectReferenceValue == null);
        }

        private static bool WireControllerReferences(
            AreaSekolahGraphicsController controller,
            VolumeProfile profile,
            Material skyboxMat)
        {
            bool changed = false;
            var volume = FindChildVolume(controller.transform);
            if (volume != null)
            {
                if (!volume.gameObject.activeSelf) volume.gameObject.SetActive(true);
                if (!volume.isGlobal) volume.isGlobal = true;
                if (profile != null && volume.sharedProfile != profile)
                    volume.sharedProfile = profile;
            }

            var dirLight = FindChildDirectionalLight(controller.transform);
            var so = new SerializedObject(controller);

            changed |= AssignObjectReferenceIfNull(so, "globalVolume", volume);
            changed |= AssignObjectReferenceIfNull(so, "mainDirectionalLight", dirLight);
            changed |= AssignObjectReferenceIfNull(so, "fullColorSkybox", skyboxMat);

            if (changed)
                so.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }

        private static bool AssignObjectReferenceIfNull(SerializedObject so, string propName, Object targetValue)
        {
            if (targetValue == null) return false;
            var prop = so.FindProperty(propName);
            if (prop == null || prop.objectReferenceValue != null) return false;
            prop.objectReferenceValue = targetValue;
            return true;
        }

        private static void RepairScene(string scenePath, Material skyboxMat)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[AreaSekolahGraphicsSetup] Scene skipped (not found): {scenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) return;

            if (skyboxMat != null)
                RenderSettings.skybox = skyboxMat;

            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.76f, 0.86f, 0.96f, 1f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 95f;
            RenderSettings.fogEndDistance = 320f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.68f, 0.78f, 0.92f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.58f, 0.6f, 0.64f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.4f, 0.37f, 1f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 1f;

            foreach (var cam in Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsInactive.Include))
            {
                if (cam == null || cam.cameraType != CameraType.Game) continue;

                var additionalData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (additionalData == null) continue;

                additionalData.renderPostProcessing = true;
                additionalData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                additionalData.antialiasingQuality = AntialiasingQuality.High;
                EditorUtility.SetDirty(additionalData);
            }

            foreach (var controller in Object.FindObjectsByType<AreaSekolahGraphicsController>(FindObjectsInactive.Include))
            {
                if (controller == null) continue;
                controller.SetFullColorGraphics(true);
                EditorUtility.SetDirty(controller);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetBoolProperty(SerializedObject so, string propName, bool value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null) prop.boolValue = value;
        }

        private static Volume FindChildVolume(Transform root)
        {
            var globalVolTransform = FindTransformByName(root, "Global Volume");
            if (globalVolTransform != null)
            {
                var v = globalVolTransform.GetComponent<Volume>();
                if (v != null) return v;
            }
            return root.GetComponentInChildren<Volume>(true);
        }

        private static Light FindChildDirectionalLight(Transform root)
        {
            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (light != null && light.type == LightType.Directional)
                    return light;
            }
            return null;
        }

        private static Transform FindTransformByName(Transform root, string targetName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child != null && child.name == targetName)
                    return child;
            }
            return null;
        }
    }
}

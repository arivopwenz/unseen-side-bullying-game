using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BullyingGame.EditorTools
{
    public static class EnvironmentOptimizer
    {
        private const string MenuRoot = "Tools/The Unseen Side/Optimizer/";

        [MenuItem(MenuRoot + "1. Terapkan URP Developer Preset (Bayangan Ringan)")]
        public static void ApplyDeveloperURPSettings()
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogWarning("[Optimizer] Active Render Pipeline is not a UniversalRenderPipelineAsset.");
                return;
            }

            Undo.RecordObject(urpAsset, "Apply Developer URP Preset");
            urpAsset.shadowDistance = 45f;
            urpAsset.shadowCascadeCount = 2;

            var so = new SerializedObject(urpAsset);
            var mainRes = so.FindProperty("m_MainLightShadowmapResolution");
            if (mainRes != null) mainRes.intValue = 1024;

            var addRes = so.FindProperty("m_AdditionalLightsShadowmapResolution");
            if (addRes != null) addRes.intValue = 512;

            var addLimit = so.FindProperty("m_AdditionalLightsPerObjectLimit");
            if (addLimit != null && addLimit.intValue > 4) addLimit.intValue = 4;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(urpAsset);
            AssetDatabase.SaveAssets();

            Debug.Log("<color=green>[Optimizer] Berhasil menerapkan URP Developer Preset!</color>\n" +
                      "• Shadow Distance: 45m (sebelumnya 140m)\n" +
                      "• Cascades: 2 (sebelumnya 4)\n" +
                      "• Main Shadow Res: 1024\n" +
                      "• Additional Shadow Res: 512");
        }

        [MenuItem(MenuRoot + "2. Kompresi FBX Lingkungan (Mesh Compression High)")]
        public static void OptimizeEnvironmentFBX()
        {
            var guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Game/assetEnv" });
            int modifiedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;

                    var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer == null) continue;

                    bool changed = false;

                    if (importer.meshCompression != ModelImporterMeshCompression.High)
                    {
                        importer.meshCompression = ModelImporterMeshCompression.High;
                        changed = true;
                    }

                    if (importer.importCameras)
                    {
                        importer.importCameras = false;
                        changed = true;
                    }

                    if (importer.importLights)
                    {
                        importer.importLights = false;
                        changed = true;
                    }

                    if (importer.isReadable)
                    {
                        importer.isReadable = false;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                        modifiedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"<color=green>[Optimizer] Selesai mengompresi {modifiedCount} file FBX lingkungan!</color> (Mesh Compression: High, Cameras/Lights: Stripped)");
        }

        [MenuItem(MenuRoot + "3. Optimasi Resolusi Tekstur Lingkungan (Max 1024)")]
        public static void OptimizeEnvironmentTextures()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Game/assetEnv", "Assets/_Game/Materials/texture" });
            int modifiedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    if (importer.maxTextureSize > 1024)
                    {
                        importer.maxTextureSize = 1024;
                        importer.SaveAndReimport();
                        modifiedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"<color=green>[Optimizer] Berhasil membatasi resolusi {modifiedCount} tekstur menjadi Max 1024px!</color>");
        }

        [MenuItem(MenuRoot + "4. Ganti MeshCollider Datar ke BoxCollider (Scene Aktif)")]
        public static void ReplaceMeshCollidersWithBoxColliders()
        {
            var meshColliders = UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include);
            int replaced = 0;

            foreach (var mc in meshColliders)
            {
                if (mc == null) continue;
                var go = mc.gameObject;
                var mf = go.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;

                // Jika objek berupa cube/box atau punya nama cube/dinding/lantai/pagar
                string nameLower = go.name.ToLower();
                bool isFlatOrBox = nameLower.Contains("cube") || nameLower.Contains("dinding") ||
                                  nameLower.Contains("pondasi") || nameLower.Contains("lantai") ||
                                  nameLower.Contains("trotoar") || nameLower.Contains("pagar") ||
                                  nameLower.Contains("kusen") || nameLower.Contains("pintu");

                if (isFlatOrBox)
                {
                    Undo.RecordObject(go, "Replace MeshCollider with BoxCollider");
                    var bounds = mf.sharedMesh.bounds;
                    UnityEngine.Object.DestroyImmediate(mc);

                    var bc = go.AddComponent<BoxCollider>();
                    bc.center = bounds.center;
                    bc.size = bounds.size;
                    replaced++;
                }
            }

            Debug.Log($"<color=green>[Optimizer] Selesai mengganti {replaced} MeshCollider menjadi BoxCollider hemat PhysX!</color>");
        }
    }
}

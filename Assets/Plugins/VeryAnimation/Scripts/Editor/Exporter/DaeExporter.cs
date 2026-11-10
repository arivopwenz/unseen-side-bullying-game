using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace VeryAnimation
{
    // STUB SEMENTARA (laptop, 2026-10-28): library Collada asli (VeryAnimation.grendgine_collada)
    // tidak ada di laptop ini (folder Library kosong), sehingga DaeExporter asli tidak bisa compile.
    // File asli dibackup (lihat pesan BACKUP di atas) - restore file itu + folder Library untuk
    // mengaktifkan kembali export DAE. Stub ini hanya agar project bisa compile; Export sengaja
    // dibuat no-op dengan peringatan.
    internal class DaeExporter
    {
        public bool settings_activeOnly = true;
        public bool settings_exportMesh = true;
        public bool settings_iKOnFeet = true;
        public bool settings_animationRigging;
        public ModelImporterAnimationType settings_animationType;
        public Avatar settings_avatar;
        public string settings_motionNodePath;
        public bool settings_AssetDatabaseRefresh = true;
        public List<string> exportedFiles = new List<string>();

        public bool Export(string path, List<Transform> transforms, AnimationClip[] clips = null)
        {
            Debug.LogWarning("[VeryAnimation] Export DAE dinonaktifkan di laptop ini karena library Collada (VeryAnimation.grendgine_collada) tidak ditemukan. Restore folder Assets/VeryAnimation/Scripts/Editor/Library/ dan file backup DaeExporter untuk mengaktifkan kembali.");
            return false;
        }
    }
}
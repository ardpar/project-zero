#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Synthborn.Core.Audio;

namespace Synthborn.Editor
{
    /// <summary>
    /// One-click setup: adds AdaptiveMusicManager to SFXManager GameObject
    /// and wires the AdaptiveMusicConfig SO. Run from menu bar.
    /// </summary>
    public static class AdaptiveMusicSetup
    {
        [MenuItem("Synthborn/Setup/Add Adaptive Music Manager")]
        public static void Setup()
        {
            // Find or create on SFXManager GameObject
            var sfxManager = Object.FindFirstObjectByType<SFXManager>();
            GameObject target;

            if (sfxManager != null)
            {
                target = sfxManager.gameObject;
            }
            else
            {
                target = new GameObject("MusicManager");
                Undo.RegisterCreatedObjectUndo(target, "Create MusicManager");
            }

            // Add component if not present
            var musicManager = target.GetComponent<AdaptiveMusicManager>();
            if (musicManager == null)
            {
                musicManager = Undo.AddComponent<AdaptiveMusicManager>(target);
            }

            // Wire config SO
            var configGuids = AssetDatabase.FindAssets("t:AdaptiveMusicConfig");
            if (configGuids.Length > 0)
            {
                var configPath = AssetDatabase.GUIDToAssetPath(configGuids[0]);
                var config = AssetDatabase.LoadAssetAtPath<AdaptiveMusicConfig>(configPath);

                var so = new SerializedObject(musicManager);
                so.FindProperty("_config").objectReferenceValue = config;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(target);
            Selection.activeGameObject = target;

            Debug.Log("[AdaptiveMusicSetup] AdaptiveMusicManager added and wired.");
        }
    }
}
#endif

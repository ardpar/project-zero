#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Synthborn.Core.Audio;

namespace Synthborn.Editor
{
    /// <summary>
    /// One-click setup menus for the adaptive music system.
    /// </summary>
    public static class AdaptiveMusicSetup
    {
        [MenuItem("Synthborn/Setup/Add Adaptive Music (Gameplay Scene)")]
        public static void SetupGameplay()
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
                musicManager = Undo.AddComponent<AdaptiveMusicManager>(target);

            WireConfig(musicManager);
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(target.scene);
            Selection.activeGameObject = target;

            Debug.Log("[AdaptiveMusicSetup] AdaptiveMusicManager added and wired to gameplay scene.");
        }

        [MenuItem("Synthborn/Setup/Add Adaptive Music (Main Menu)")]
        public static void SetupMainMenu()
        {
            var target = new GameObject("MainMenuMusic");
            Undo.RegisterCreatedObjectUndo(target, "Create MainMenuMusic");

            var menuMusic = Undo.AddComponent<MainMenuMusic>(target);
            WireConfig(menuMusic);
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(target.scene);
            Selection.activeGameObject = target;

            Debug.Log("[AdaptiveMusicSetup] MainMenuMusic added and wired to menu scene.");
        }

        private static void WireConfig(Component component)
        {
            var configGuids = AssetDatabase.FindAssets("t:AdaptiveMusicConfig");
            if (configGuids.Length == 0) return;

            var configPath = AssetDatabase.GUIDToAssetPath(configGuids[0]);
            var config = AssetDatabase.LoadAssetAtPath<AdaptiveMusicConfig>(configPath);

            var so = new SerializedObject(component);
            so.FindProperty("_config").objectReferenceValue = config;
            so.ApplyModifiedProperties();
        }
    }
}
#endif

using System.IO;
using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Handles JSON save/load to persistent data path.
    /// </summary>
    public static class SaveManager
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "synthborn_save.json");
        private static SaveData _cached;

        /// <summary>Get current save data (cached after first load).</summary>
        public static SaveData Data
        {
            get
            {
                if (_cached == null) Load();
                return _cached;
            }
        }

        public static void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_cached ?? new SaveData(), true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    _cached = JsonUtility.FromJson<SaveData>(json);
                }
                else
                {
                    _cached = CreateDefaultSave();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveManager] Load failed, using defaults: {e.Message}");
                _cached = CreateDefaultSave();
            }
        }

        private static SaveData CreateDefaultSave()
        {
            var data = new SaveData();
            // Default unlocked mutations (starter set)
            data.unlockedMutationIds.AddRange(new[]
            {
                "IronSkin", "QuickStep", "SharpClaws", "Vitality",
                "BladeArms", "SprintLegs", "Haste", "CriticalMass"
            });
            return data;
        }

        /// <summary>Reset save data to defaults.</summary>
        public static void ResetSave()
        {
            _cached = CreateDefaultSave();
            Save();
        }
    }
}

using System.IO;
using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Handles JSON save/load with 3 save slots.
    /// Each slot stores both global SaveData and CharacterSaveData.
    /// </summary>
    public static class SaveManager
    {
        private static readonly string SaveDir = Application.persistentDataPath;
        private static SaveData _cached;
        private static CharacterSaveData _character;
        private static int _activeSlot = -1;

        /// <summary>Get current global save data (cached after first load).</summary>
        public static SaveData Data
        {
            get
            {
                if (_cached == null) LoadGlobal();
                return _cached;
            }
        }

        /// <summary>Active character data. Null if no slot loaded.</summary>
        public static CharacterSaveData Character => _character;

        /// <summary>Currently active save slot (0-2). -1 if none.</summary>
        public static int ActiveSlot => _activeSlot;

        // ─── Global Save (settings, unlocks — shared across slots) ───

        private static string GlobalPath => Path.Combine(SaveDir, "synthborn_save.json");

        public static void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_cached ?? new SaveData(), true);
                File.WriteAllText(GlobalPath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Global save failed: {e.Message}");
            }
        }

        private static void LoadGlobal()
        {
            try
            {
                if (File.Exists(GlobalPath))
                {
                    string json = File.ReadAllText(GlobalPath);
                    _cached = JsonUtility.FromJson<SaveData>(json);
                }
                else
                {
                    _cached = CreateDefaultSave();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveManager] Global load failed: {e.Message}");
                _cached = CreateDefaultSave();
            }
        }

        // ─── Character Slot Save (per-slot) ───

        private static string SlotPath(int slot) =>
            Path.Combine(SaveDir, $"synthborn_slot{slot}.json");

        /// <summary>Load a character save slot. Returns true if slot exists.</summary>
        public static bool LoadSlot(int slot)
        {
            _activeSlot = slot;
            string path = SlotPath(slot);
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    _character = JsonUtility.FromJson<CharacterSaveData>(json);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveManager] Slot {slot} load failed: {e.Message}");
            }
            _character = null;
            return false;
        }

        /// <summary>Save the active character slot.</summary>
        public static void SaveSlot()
        {
            if (_activeSlot < 0 || _character == null) return;
            try
            {
                _character.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                string json = JsonUtility.ToJson(_character, true);
                File.WriteAllText(SlotPath(_activeSlot), json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Slot {_activeSlot} save failed: {e.Message}");
            }
        }

        /// <summary>Create a new character in a slot.</summary>
        public static void CreateCharacter(int slot, string name, int classType)
        {
            _activeSlot = slot;
            _character = new CharacterSaveData
            {
                characterName = name,
                classType = classType,
                characterLevel = 1,
                highestLevelUnlocked = 1,
                lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };
            SaveSlot();
        }

        /// <summary>Check if a save slot has data.</summary>
        public static bool SlotExists(int slot) => File.Exists(SlotPath(slot));

        /// <summary>Peek at a slot's data without loading it as active.</summary>
        public static CharacterSaveData PeekSlot(int slot)
        {
            string path = SlotPath(slot);
            if (!File.Exists(path)) return null;
            try
            {
                return JsonUtility.FromJson<CharacterSaveData>(File.ReadAllText(path));
            }
            catch { return null; }
        }

        /// <summary>Delete a save slot.</summary>
        public static void DeleteSlot(int slot)
        {
            string path = SlotPath(slot);
            if (File.Exists(path)) File.Delete(path);
            if (_activeSlot == slot) { _activeSlot = -1; _character = null; }
        }

        // ─── Defaults ───

        private static SaveData CreateDefaultSave()
        {
            var data = new SaveData();
            data.unlockedMutationIds.AddRange(new[]
            {
                "IronSkin", "QuickStep", "SharpClaws", "Vitality",
                "BladeArms", "SprintLegs", "Haste", "CriticalMass"
            });
            return data;
        }

        /// <summary>Reset global save data to defaults.</summary>
        public static void ResetSave()
        {
            _cached = CreateDefaultSave();
            Save();
        }
    }
}

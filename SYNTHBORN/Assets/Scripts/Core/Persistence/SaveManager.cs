using System.IO;
using UnityEngine;

namespace Synthborn.Core.Persistence
{
    /// <summary>
    /// Handles JSON save/load with 3 save slots.
    /// Each slot stores both global SaveData and CharacterSaveData.
    ///
    /// Safety features:
    /// - Save versioning with forward-compatible migration
    /// - Write-to-temp-then-rename (prevents corruption on crash)
    /// - Automatic backup restore on load failure
    /// </summary>
    public static class SaveManager
    {
        /// <summary>Current save format version. Increment when adding/changing fields.</summary>
        public const int CurrentVersion = 1;

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

        // ─── Global Save ───

        private static string GlobalPath => Path.Combine(SaveDir, "synthborn_save.json");

        public static void Save()
        {
            try
            {
                if (_cached == null) _cached = CreateDefaultSave();
                _cached.saveVersion = CurrentVersion;
                string json = JsonUtility.ToJson(_cached, true);
                SafeWriteFile(GlobalPath, json);
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
                    MigrateGlobalSave(_cached);
                }
                else
                {
                    _cached = CreateDefaultSave();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveManager] Global load failed, trying backup: {e.Message}");
                _cached = TryRestoreBackup<SaveData>(GlobalPath) ?? CreateDefaultSave();
            }
        }

        // ─── Character Slot Save ───

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
                    MigrateCharacterSave(_character);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveManager] Slot {slot} load failed, trying backup: {e.Message}");
                _character = TryRestoreBackup<CharacterSaveData>(path);
                if (_character != null) { MigrateCharacterSave(_character); return true; }
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
                _character.saveVersion = CurrentVersion;
                _character.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                string json = JsonUtility.ToJson(_character, true);
                SafeWriteFile(SlotPath(_activeSlot), json);
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
                saveVersion = CurrentVersion,
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
                var data = JsonUtility.FromJson<CharacterSaveData>(File.ReadAllText(path));
                MigrateCharacterSave(data);
                return data;
            }
            catch { return null; }
        }

        /// <summary>Delete a save slot and its backup.</summary>
        public static void DeleteSlot(int slot)
        {
            string path = SlotPath(slot);
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".bak")) File.Delete(path + ".bak");
            if (_activeSlot == slot) { _activeSlot = -1; _character = null; }
        }

        // ─── Safe Write (write-to-temp-then-rename) ───

        private static void SafeWriteFile(string path, string json)
        {
            string tmpPath = path + ".tmp";
            string bakPath = path + ".bak";

            // 1. Write to temp
            File.WriteAllText(tmpPath, json);

            // 2. Backup existing
            if (File.Exists(path))
            {
                if (File.Exists(bakPath)) File.Delete(bakPath);
                File.Move(path, bakPath);
            }

            // 3. Promote temp to live
            File.Move(tmpPath, path);
        }

        private static T TryRestoreBackup<T>(string path) where T : class
        {
            string bakPath = path + ".bak";
            if (!File.Exists(bakPath)) return null;
            try
            {
                Debug.LogWarning($"[SaveManager] Restoring from backup: {bakPath}");
                return JsonUtility.FromJson<T>(File.ReadAllText(bakPath));
            }
            catch
            {
                Debug.LogError($"[SaveManager] Backup restore also failed: {bakPath}");
                return null;
            }
        }

        // ─── Version Migration ───

        private static void MigrateGlobalSave(SaveData data)
        {
            if (data == null) return;

            if (data.saveVersion < 1)
            {
                // v0→v1: saveVersion field added (JsonUtility defaults to 0 for missing int)
                data.saveVersion = 1;
                Debug.Log("[SaveManager] Migrated global save v0 → v1");
            }
            // Future: if (data.saveVersion < 2) { ... data.saveVersion = 2; }
        }

        private static void MigrateCharacterSave(CharacterSaveData data)
        {
            if (data == null) return;

            if (data.saveVersion < 1)
            {
                // v0→v1: signalArchiveEntries + adaptationAllocated added
                if (data.signalArchiveEntries == null)
                    data.signalArchiveEntries = new System.Collections.Generic.List<string>();
                if (data.adaptationAllocated == null || data.adaptationAllocated.Length < 5)
                    data.adaptationAllocated = new int[5];
                data.saveVersion = 1;
                Debug.Log("[SaveManager] Migrated character save v0 → v1");
            }
        }

        // ─── Defaults ───

        private static SaveData CreateDefaultSave()
        {
            var data = new SaveData { saveVersion = CurrentVersion };
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

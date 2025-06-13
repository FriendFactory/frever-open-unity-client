using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Modules.LocalStorage
{
    public static class LocalStorageManager
    {
        private static readonly Dictionary<string, long> SEQUENCE = new Dictionary<string, long>();
        private static readonly string DATA_PATH = Application.persistentDataPath;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static long GetNextLocalId(string name)
        {
            if (SEQUENCE.Count == 0)
            {
                CheckSavedSequence();
            }

            UpdateSequence(name);

            return SEQUENCE[name];
        }

        public static string GetLocalStoragePath()
        {
            return $"{DATA_PATH}/LocalStorage";
        }

        public static string GetLevelPath(long levelId)
        {
            var prefix = levelId >= 0 ? "_R" : "_L";
            var id = Math.Abs(levelId);
            return $"{GetLocalStoragePath()}/Level{prefix}{id}";
        }

        public static string GetEventPath(long levelId, long eventId)
        {
            var prefix = eventId >= 0 ? "_R" : "_L";
            var id = Math.Abs(eventId);
            return $"{GetLevelPath(levelId)}/Event{prefix}{id}";
        }

        public static void DeleteFiles()
        {
            var dir = new DirectoryInfo(GetLocalStoragePath());
            if (!dir.Exists) return;

            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (var subdir in dir.GetDirectories())
            {
                subdir.Delete(true);
            }
            SEQUENCE.Clear();
            SaveSequenceToFile();
        }

        private static void UpdateSequence(string name)
        {
            if (SEQUENCE.TryGetValue(name, out var value))
            {
                SEQUENCE[name] = value - 1;
            }
            else
            {
                SEQUENCE.Add(name, -1);
            }
            SaveSequenceToFile();
        }

        private static string GetSequencePath()
        {
            return Path.Combine(GetLocalStoragePath(), "sequence.json");
        }

        private static void SaveSequenceToFile()
        {
            var storagePath = GetLocalStoragePath();
            var storageDir = new DirectoryInfo(storagePath);
            if (!storageDir.Exists) Directory.CreateDirectory(storagePath);

            var sequenceJson = JsonConvert.SerializeObject(SEQUENCE);
            using (var stream = File.CreateText(GetSequencePath()))
            {
                stream.WriteAsync(sequenceJson);
            }
        }

        private static void CheckSavedSequence()
        {
            var sequenceFilePath = GetSequencePath();
            if (!File.Exists(sequenceFilePath)) return;

            var json = File.ReadAllText(sequenceFilePath);
            var loadedSequence = JsonConvert.DeserializeObject<Dictionary<string, long>>(json);
            if (loadedSequence == null || loadedSequence.Count == 0) return;

            foreach (var kv in loadedSequence)
            {
                SEQUENCE.Add(kv.Key, kv.Value);
            }
        }
    }
}

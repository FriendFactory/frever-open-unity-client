using System;
using System.IO;
using JetBrains.Annotations;
using Modules.LocalStorage;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.TempSaves.Manager
{
    [UsedImplicitly]
    public sealed class TempFileManager
    {
        public TempFileManager()
        {
            LocalStorageManager.GetLocalStoragePath(); //Force init for LocalStorageManager
        }

        public void SaveDataLocally<TData>(TData data, string path)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            SaveTextFile(jsonData, path);
        }

        public void RemoveTempFile(string path)
        {
            var totalPath = Path.Combine(Application.persistentDataPath, path);
            if (!File.Exists(totalPath)) return;
            File.Delete(totalPath);
        }

        public bool FileExists(string path)
        {
            var fileFullPath = GetFullPath(path);

            return File.Exists(fileFullPath);
        }

        public TData GetData<TData>(string path) where TData: class
        {
            var fileFullPath = GetFullPath(path);
            
            if (!File.Exists(fileFullPath))
            {
                return null;
            }

            var dataText = File.ReadAllText(fileFullPath);
            TData data = null;
            try
            {
                data = JsonConvert.DeserializeObject<TData>(dataText);
            }
            catch (Exception)
            {
                return null;
            }

            return data;
        }

        private void SaveTextFile(string data, string filePath)
        {
            var path = GetFullPath(filePath);
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, data);
        }

        private string GetFullPath(string filePath)
        {
            var path = Application.persistentDataPath;
            path = Path.Combine(path, filePath);
            return path;
        }
    }
}
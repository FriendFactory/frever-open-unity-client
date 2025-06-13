using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using UnityEngine;

namespace Modules.CameraSystem.CameraAnimations
{
    [UsedImplicitly]
    internal sealed class CameraAnimationSaver
    {
        private readonly string _cacheFolder;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public CameraAnimationSaver()
        {
            _cacheFolder = Path.Combine(Application.persistentDataPath, Constants.FileDefaultPaths.CAMERA_ANIMATIONS_FOLDER);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public string SaveTextFileFromString(string text, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = GetUniqueCameraAnimationFileName();
            }
            var filePath = GetFilePath(fileName);
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(filePath))
            {
                throw new InvalidOperationException("Can't save camera animation. Camera animation text or file path is null or empty");
            }
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var file = File.Create(filePath))
            {
                var info = new UTF8Encoding(true).GetBytes(text);
                file.Write(info, 0, info.Length);
            }

            return filePath;
        }

        public async Task CleanCacheAsync()
        {
            await Task.Run(CleanCache);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void CleanCache()
        {
            if (!Directory.Exists(_cacheFolder))
                return;
            
            var di = new DirectoryInfo(_cacheFolder);

            foreach (var file in di.GetFiles())
            {
                file.Delete(); 
            }
            foreach (var dir in di.GetDirectories())
            {
                dir.Delete(true); 
            }
        }
        
        private string GetFilePath(string fileName)
        {
            return Path.Combine(_cacheFolder, fileName);
        }
        
        public string GetUniqueCameraAnimationFileName()
        {
            return $"{Guid.NewGuid()}.txt";
        }
    }
}
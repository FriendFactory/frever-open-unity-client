using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge;
using Common.ApplicationCore;
using Newtonsoft.Json;
using SimpleDiskUtils;
using UnityEngine;

namespace Modules.AssetsManaging.UncompressedBundles
{
    internal sealed class DecompressedBundlesStorage
    {
        private const float RECOMMENDED_STORAGE_USAGE_LIMIT_MBS = 500;
        private const int RECOMMENDED_MINIMUM_MINUTES_TO_KEEP_ASSET = 60;
        private const int AVAILABLE_DISK_SPACE_THRESHOLD = 300;
        
        private readonly List<UnpackedBundleInfo> _decompressedBundles = new List<UnpackedBundleInfo>();
        private readonly ISessionInfo _sessionInfo;
        private readonly string _persistentDataPath = Application.persistentDataPath;//need to cache for invoking from another thread
        private readonly object _lockerFile = new object();
        private readonly IEncryptionBridge _encryptionService;

        private bool _metadataLoaded;
        
        private string StorageFolder => $"{_persistentDataPath}/DecompressedBundles/{_sessionInfo.Environment}";
        private string MetaDataFilePath => $"{StorageFolder}/UncompressedBundlesData.json";
        
        public float StorageUsageMbs { get; private set; }
        
        public DecompressedBundlesStorage(ISessionInfo sessionInfo, IEncryptionBridge encryptionService)
        {
            _sessionInfo = sessionInfo;
            _encryptionService = encryptionService;
        }

        public string GetPath(string modelType, long modelId, string version)
        {
            var path = $"{StorageFolder}/{modelType}/{modelId}/{version}";
            return _encryptionService.EncryptionEnabled ? $"{path}{_encryptionService.TargetExtension}" : path; //dwc
        }

        public UnpackedBundleInfo GetData(string modelTypeName, long modelId)
        {
            if (!_metadataLoaded)
            {
                 LoadMetadata();
            }
            return _decompressedBundles.FirstOrDefault(x => x.ModelId == modelId && x.ModelTypeName == modelTypeName);
        }

        public void Delete(UnpackedBundleInfo bundleInfo)
        {
            _decompressedBundles.Remove(bundleInfo);
            StorageUsageMbs -= bundleInfo.FileSizeMb;
            var path = GetPath(bundleInfo.ModelTypeName, bundleInfo.ModelId, bundleInfo.FileVersion);
            DeleteFileAsync(path);
        }

        private async void DeleteFileAsync(string filePath)
        {
            if (!File.Exists(filePath)) return;
            
            await Task.Run(() =>
            {
                File.Delete(filePath);
                
                var dir = Path.GetDirectoryName(filePath);
                var remainingFiles = Directory.GetFiles(dir);
                var isDirectoryEmpty = remainingFiles.Length == 0;
                if (!isDirectoryEmpty) return;
                Directory.Delete(dir);
            });
        }

        public void Add(UnpackedBundleInfo bundleInfo)
        {
            if (_decompressedBundles.Contains(bundleInfo)) return;
            _decompressedBundles.Add(bundleInfo);
            StorageUsageMbs += bundleInfo.FileSizeMb;

            TryToCleanupStorage();
        }

        public async void SaveMetadata()
        {
            await Task.Run(() =>
            {
                var json = JsonConvert.SerializeObject(_decompressedBundles);
                var dir = Path.GetDirectoryName(MetaDataFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                WriteToFile(MetaDataFilePath, json);
            });
        }
        
        public void LoadMetadata()
        {
            if (_metadataLoaded) return;
            
            _decompressedBundles.Clear();
            StorageUsageMbs = 0f;

            if (!File.Exists(MetaDataFilePath))
            {
                _metadataLoaded = true;
                return;
            }
            
            if (!File.Exists(MetaDataFilePath)) return;

            if (_sessionInfo.PreviousSessionCrashed)
            {
                CleanAndReset();
                return;
            }
            
            var json = File.ReadAllText(MetaDataFilePath);
            var metaData = JsonConvert.DeserializeObject<UnpackedBundleInfo[]>(json);
            _decompressedBundles.AddRange(metaData);
            StorageUsageMbs = _decompressedBundles.Sum(x => x.FileSizeMb);
            _metadataLoaded = true;
        }

        private void TryToCleanupStorage()
        {
            var needToCleanup = StorageUsageMbs > RECOMMENDED_STORAGE_USAGE_LIMIT_MBS;
            if (!needToCleanup) return;
            
            var overMbs = StorageUsageMbs - RECOMMENDED_STORAGE_USAGE_LIMIT_MBS;

            var orderedBundles = _decompressedBundles.OrderBy(x => x.LastUsedTime).ToArray();
            foreach (var bundle in orderedBundles)
            {
                if (!ShouldDropBundle(bundle)) break;
                
                overMbs -= bundle.FileSizeMb;
                Delete(bundle);
                
                var isDroppedEnough = overMbs <= 0;
                if (isDroppedEnough) break;
            }
        }

        public void CleanCache()
        {
            for (var i = _decompressedBundles.Count - 1; i >= 0; i--)
            {
                var bundle = _decompressedBundles[i];
                Delete(bundle);
            }
        }
        
        private void CleanAndReset()
        {
            var rootDir = Path.GetDirectoryName(MetaDataFilePath);
            if (Directory.Exists(rootDir))
            {            
                Directory.Delete(rootDir, true);
            }
        }

        private void WriteToFile(string filePath, string content)
        {
            lock (_lockerFile)
            {
                using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fileStream, Encoding.Unicode))
                {
                    fileStream.SetLength(0);
                    writer.Write(content);
                }
            }
        }

        private bool ShouldDropBundle(UnpackedBundleInfo bundle)
        {
            //drop bundle if it was not used recently (during last hour)
            var lastUsedTimeRecommendedToDrop = DateTime.Now - TimeSpan.FromMinutes(RECOMMENDED_MINIMUM_MINUTES_TO_KEEP_ASSET);
            if (bundle.LastUsedTime < lastUsedTimeRecommendedToDrop)
            {
                return true;
            }
            
            var freeDiskSpace = DiskUtils.CheckAvailableSpace();
            return freeDiskSpace < AVAILABLE_DISK_SPACE_THRESHOLD;
        }
    }
}
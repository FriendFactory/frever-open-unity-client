using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common.Files;
using Bridge.Models.Common;
using Bridge;
using Bridge.Exceptions;
using Common.ApplicationCore;
using Extensions;
using JetBrains.Annotations;
using UnityEngine;
using Utils;

namespace Modules.AssetsManaging.UncompressedBundles
{
    [UsedImplicitly]
    public sealed class UncompressedBundlesManager
    {
        private readonly IBridgeCache _bridgeCache;
        private readonly DecompressedBundlesStorage _storage;
        private readonly BundleDecompressor _decompressor;
        private readonly AssetBundleLoader _bundleLoader;
        private readonly List<IEntity> _decompressingNow = new List<IEntity>();
        private readonly IEncryptionBridge _encryptionService;

        private bool EncryptionEnabled => _encryptionService.EncryptionEnabled;

        public UncompressedBundlesManager(IBridgeCache cache, ISessionInfo sessionInfo, IEncryptionBridge encryptionService)
        {
            _bridgeCache = cache;
            _encryptionService = encryptionService;
            _storage = new DecompressedBundlesStorage(sessionInfo, _encryptionService);
            _decompressor = new BundleDecompressor(_encryptionService);
            _bundleLoader = new AssetBundleLoader(_encryptionService);
            
#if UNITY_EDITOR
            void OnEditorModeChanged(UnityEditor.PlayModeStateChange stateChange)
            {
                if (stateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    UnityEditor.EditorApplication.playModeStateChanged -= OnEditorModeChanged;
                    _storage.SaveMetadata();
                }
            }

            UnityEditor.EditorApplication.playModeStateChanged += OnEditorModeChanged;
#else
            Application.focusChanged += isOn =>
            {
                if (isOn) return;
                _storage.SaveMetadata();
            };
#endif
            
            _storage.LoadMetadata();
        }

        public async Task DecompressBundle<T>(T model, CancellationToken token = default) where T:IMainFileContainable
        {
            if (IsDecompressingNow(model))
            {
                return;
            }
            _decompressingNow.Add(model);

            var fileInfo = GetPlatformBasedFileInfo(model.Files); 
            var modelTypeName = GetModelTypeName(model);
            var destinationPath = _storage.GetPath(modelTypeName, model.Id, fileInfo.Version);

            var bundleInfo = _storage.GetData(modelTypeName, model.Id);
            
            if (bundleInfo != null && bundleInfo.FileVersion == fileInfo.Version)
            {
                //todo: drop this "if" block in autumn 2023, because it was made for smooth transition during encryption integration
                if (EncryptionEnabled && !bundleInfo.Encrypted)
                {
                    try
                    {
                        await _decompressor.EncryptDecompressedFileAsync(destinationPath, token);
                        bundleInfo.Encrypted = true;
                    }
                    catch (FileEncryptionException e)
                    {
                        Debug.LogError(FileEncryptionException.BuildErrorMessage( $"Failed to encrypt decompressed bundle: {e.Message}"));
                    }
                }
                
                _decompressingNow.Remove(model);
                return;
            }
            
            var isUncompressedBundleHasOlderVersion = bundleInfo != null && bundleInfo.FileVersion != fileInfo.Version;
            if (isUncompressedBundleHasOlderVersion)
            {
                _storage.Delete(bundleInfo);
            }
            
            var sourceFileData = await _bridgeCache.GetCachedFileDataAsync(model, fileInfo);
            if (sourceFileData == null)
            {
                Debug.LogWarning($"Source bundle for decompressing does not exists. {modelTypeName} {model.Id}");
                _decompressingNow.Remove(model);
                return;
            }

            try
            {
                var sourcePath = _bridgeCache.GetCachedFileFullPath(sourceFileData);
                await _decompressor.DecompressAsync(sourcePath, destinationPath, token);
                
                bundleInfo = new UnpackedBundleInfo
                {
                    ModelId = model.Id,
                    ModelTypeName = GetModelTypeName(model),
                    FileVersion = fileInfo.Version,
                    LastUsedTime = DateTime.Now,
                    FileSizeMb = GetFileSizeMb(destinationPath),
                    Encrypted = EncryptionEnabled
                };

                _storage.Add(bundleInfo);
            }
            catch (FileEncryptionException e)
            {
                Debug.LogError(FileEncryptionException.BuildErrorMessage($"Failed to decompress encrypted bundle: {e.Message}"));
            }
            finally
            {
                _decompressingNow.Remove(model);
            }
        }

        public bool IsDecompressingNow<T>(T entity) where T : IFilesAttachedEntity
        {
            return _decompressingNow.Any(x => x.Id == entity.Id && x.GetModelType() == entity.GetModelType());
        }
        
        public Task<AssetBundle> GetUncompressedBundleIfExists<T>(T model, CancellationToken cancellationToken = default) where T:IMainFileContainable
        {
            var modelTypeName = GetModelTypeName(model);
            var bundleData = _storage.GetData(modelTypeName, model.Id);
            if (bundleData == null) return Task.FromResult<AssetBundle>(null);

            var bundlePath = _storage.GetPath(bundleData.ModelTypeName, bundleData.ModelId, bundleData.FileVersion);
            bundleData.LastUsedTime = DateTime.Now;
            return _bundleLoader.Load(bundlePath, cancellationToken);
        }
        
        public string GetUnpackedBundlePath<T>(T model) where T:IMainFileContainable
        {
            var modelTypeName = GetModelTypeName(model);
            var bundleData = _storage.GetData(modelTypeName, model.Id);
            return bundleData == null ? null : _storage.GetPath(bundleData.ModelTypeName, bundleData.ModelId, bundleData.FileVersion);
        }

        public float GetStorageUseMbs()
        {
            return _storage.StorageUsageMbs;
        }

        private float GetFileSizeMb(string filePath)
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            var bytes = fileInfo.Length;
            return bytes / 1024f / 1024f;
        }

        private string GetModelTypeName(IEntity entity)
        {
            return entity.GetModelType().ToString();
        }

        public void CleanCache()
        {
            _storage.CleanCache();
        }

        private FileInfo GetPlatformBasedFileInfo(IEnumerable<FileInfo> files) =>
            PlatformUtils.GetRuntimePlatform() == RuntimePlatform.Android
                ? files.First(x => x.FileType == FileType.MainFile && x.Platform == Platform.Android)
                : files.First(x => x.FileType == FileType.MainFile);
    }
}

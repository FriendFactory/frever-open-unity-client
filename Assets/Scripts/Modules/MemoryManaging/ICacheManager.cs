using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Common.Exceptions;
using Modules.Amplitude;
using JetBrains.Annotations;
using SimpleDiskUtils;
using UnityEngine;

namespace Modules.MemoryManaging
{
    public interface ICacheManager
    {
        void ClearCacheIfNeeded(ICollection<IFilesAttachedEntity> assetsToKeep = null);
    }

    /// <summary>
    /// Responsible for cleaning the cache
    /// It can check the disk space with constant interval and delete files if free disk space is lower than threshold
    /// For deletion it tries to cleanup the oldest file. If it's not enough, it will delete more recently used
    /// </summary>
    [UsedImplicitly]
    internal sealed class CacheManager: ICacheManager
    {
        private const int CLEAN_CACHE_SPACE_THRESHOLD_MB = 250;
        private const int FREE_DISK_SPACE_MIN = 150;

        private static readonly Type[] ASSET_TYPES_TO_KEEP = { typeof(Watermark) };
        private readonly string[] _assetTypesToKeepNames;

        private static readonly TimeSpan FORCE_DELETE_LAST_TIME_USAGE_THRESHOLD = TimeSpan.FromDays(4);
        
        private static readonly TimeSpan[] FILES_GENERATION_BASED_ON_LAST_TIME_USAGE =
        {
            TimeSpan.FromDays(1), TimeSpan.FromHours(6), TimeSpan.FromHours(1)
        };

        private readonly IBridgeCache _bridgeCache;
        private readonly AmplitudeManager _amplitudeManager;
        private bool _cleaning;

        private int AvailableDiskSpace => DiskUtils.CheckAvailableSpace();
        private bool HasEnoughDiskSpace => AvailableDiskSpace > CLEAN_CACHE_SPACE_THRESHOLD_MB;

        public CacheManager(IBridgeCache bridgeCache, AmplitudeManager amplitudeManager)
        {
            _bridgeCache = bridgeCache;
            _amplitudeManager = amplitudeManager;
            _assetTypesToKeepNames = ASSET_TYPES_TO_KEEP.Select(x => _bridgeCache.GetAssetTypeUnifiedName(x)).ToArray();
        }

        public async void ClearCacheIfNeeded(ICollection<IFilesAttachedEntity> assetsToKeep = null)
        {
            await DeleteFiles(assetsToKeep, FORCE_DELETE_LAST_TIME_USAGE_THRESHOLD);
            
            if (HasEnoughDiskSpace) return;

            if (_cleaning) return;
            _cleaning = true;

            await DeleteFilesStartingFromOldest(assetsToKeep);

            if (!HasEnoughDiskSpace && AvailableDiskSpace <= FREE_DISK_SPACE_MIN)
            {
                Debug.LogError(ErrorConstants.LOW_DISK_SPACE_ERROR_MESSAGE);
            }
            _cleaning = false;
        }
        
        private async Task DeleteFilesStartingFromOldest(ICollection<IFilesAttachedEntity> tryToKeepAssets = null)
        {
            foreach (var lastUsageThreshold in FILES_GENERATION_BASED_ON_LAST_TIME_USAGE)
            {
                await DeleteFiles(tryToKeepAssets, lastUsageThreshold);

                if (HasEnoughDiskSpace) break;
            }
        }

        private async Task DeleteFiles(ICollection<IFilesAttachedEntity> tryToKeepAssets, TimeSpan lastUsageThreshold)
        {
            var utcNow = DateTime.UtcNow;
            var notUsedForAWhileFiles = _bridgeCache.GetCachedFilesData().Where(x => utcNow - x.LastUsedDateUTC > lastUsageThreshold);

            if (tryToKeepAssets != null)
            {
                notUsedForAWhileFiles = notUsedForAWhileFiles.Where(fileData => !tryToKeepAssets.Any(
                                                                        asset => asset.Id == fileData.AssetId
                                                                         && fileData.AssetTypeName == GetAssetTypeName(asset)));
            }

            var filesToDrop = notUsedForAWhileFiles.Where(x=> !_assetTypesToKeepNames.Contains(x.AssetTypeName)).ToArray();
            
            if (filesToDrop.Length == 0) return;
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CACHE_AUTO_CLEARING, new Dictionary<string, object>
            {
                { AmplitudeEventConstants.EventProperties.TIME_RANGE, lastUsageThreshold.TotalMinutes },
                { AmplitudeEventConstants.EventProperties.AVAILABLE_DISK_SPACE, AvailableDiskSpace }
            });
            await _bridgeCache.DeleteFromCache(filesToDrop);
        }

        private string GetAssetTypeName(IFilesAttachedEntity entity)
        {
            return _bridgeCache.GetAssetTypeUnifiedName(entity);
        }
    }
}
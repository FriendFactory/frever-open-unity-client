using System.Linq;
using Bridge;
using Common.ApplicationCore;
using JetBrains.Annotations;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.PageLoadTracking;
using Navigation.Core;
using Zenject;

namespace Modules.CrashRecovery
{
    [UsedImplicitly]
    internal sealed class AppStuckCacheCleaner: IInitializable
    {
        private const int LOADING_STUCK_THRESHOLD_MS = 15 * 1000;
        
        private readonly IPageLoadTimeTracker[] _pageLoadTimeTrackers;
        private readonly IBridgeCache _bridgeCache;
        private readonly UncompressedBundlesManager _uncompressedBundlesManager;
        private readonly IAppEventsSource _appEventsSource;

        private readonly LoadTimeTrackerType[] _clearCacheOnStuckLoadings = new[]
        {
            LoadTimeTrackerType.LevelEditor, LoadTimeTrackerType.UmaEditor
        };

        public AppStuckCacheCleaner(IPageLoadTimeTracker[] pageLoadTimeTrackers, IBridgeCache bridgeCache, UncompressedBundlesManager uncompressedBundlesManager, IAppEventsSource appEventsSource)
        {
            _pageLoadTimeTrackers = pageLoadTimeTrackers;
            _bridgeCache = bridgeCache;
            _uncompressedBundlesManager = uncompressedBundlesManager;
            _appEventsSource = appEventsSource;
        }

        public void Initialize()
        {
            foreach (var tracker in _pageLoadTimeTrackers.Where(x => _clearCacheOnStuckLoadings.Contains(x.Type)))
            {
                tracker.TrackingStarted += OnTrackingStarted;
                tracker.TrackingEnded += OnTrackingEnded;
            }
        }

        private void OnTrackingStarted(PageArgs pageArgs)
        {
            _appEventsSource.ApplicationFocused -= OnApplicationFocusChanged;
            _appEventsSource.ApplicationFocused += OnApplicationFocusChanged;
        }
        
        private void OnTrackingEnded(PageArgs pageArgs, long elapsedMs)
        {
            _appEventsSource.ApplicationFocused -= OnApplicationFocusChanged;
        }

        private void OnApplicationFocusChanged(bool isFocused)
        {
            if (isFocused) return;
            
            var targetPageTracker = _pageLoadTimeTrackers.First(x => x.IsPageLoading);
            if (targetPageTracker.ElapsedMs > LOADING_STUCK_THRESHOLD_MS)
            {
                ClearCache();
            }
        }

        private async void ClearCache()
        {
            _uncompressedBundlesManager.CleanCache();
            await _bridgeCache.ClearAssetBundleAsync();
        }
    }
}
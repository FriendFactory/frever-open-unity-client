using Bridge;
using Common;
using JetBrains.Annotations;

namespace Modules.FeaturesOpening
{
    /// <summary>
    /// Fetches images necessary for locked feature popups etc
    /// </summary>
    [UsedImplicitly]
    internal sealed class LockedFeaturesFilesLoader
    {
        private readonly IBridge _bridge;

        private static string LockedFeatureImageKey => Constants.FileKeys.LEVEL_CREATION_LOCKED_BG;
        private static string UnlockedFeatureImageKey => Constants.FileKeys.LEVEL_CREATION_UNLOCKED_BG;

        public LockedFeaturesFilesLoader(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async void Fetch()
        {
            await _bridge.FetchImageAsync(LockedFeatureImageKey);
            await _bridge.FetchImageAsync(UnlockedFeatureImageKey);
        }
    }
}
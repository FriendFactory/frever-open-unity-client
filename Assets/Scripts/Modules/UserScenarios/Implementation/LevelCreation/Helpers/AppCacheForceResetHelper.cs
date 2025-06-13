using Bridge;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.LevelManaging.Editing.LevelManagement;

namespace Modules.UserScenarios.Implementation.LevelCreation.Helpers
{
    /// <summary>
    /// Used for stuck on loading screens to prevent any problems with broken asset files
    /// </summary>
    [UsedImplicitly]
    internal sealed class AppCacheForceResetHelper
    {
        private readonly ILevelManager _levelManager;
        private readonly IBridge _bridge;
        private readonly UncompressedBundlesManager _uncompressedBundlesManager;

        public AppCacheForceResetHelper(ILevelManager levelManager, IBridge bridge, UncompressedBundlesManager uncompressedBundlesManager)
        {
            _levelManager = levelManager;
            _bridge = bridge;
            _uncompressedBundlesManager = uncompressedBundlesManager;
        }

        public void CancelAllLoadings()
        {
            _levelManager.CancelLoadingCurrentAssets();
            _bridge.CancelAllFileLoadingProcesses();
        }

        public async void ClearCache()
        {
            _uncompressedBundlesManager.CleanCache();
            await _bridge.ClearAssetBundleAsync();
        }
    }
}

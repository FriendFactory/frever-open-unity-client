using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class CameraAnimationAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.CameraAnimation;

        public CameraAnimationAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            if (ev.CameraController == null || ev.CameraController.Count == 0)
                return Empty;

            var cameraAnimationId = ev.GetCameraAnimationId();
            var loadedCameraAnim = AssetManager.GetAllLoadedAssets(DbModelType.CameraAnimation);
            return loadedCameraAnim.Where(x => cameraAnimationId == x.Id).ToArray();
        }
    }
}
using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class CameraFilterVariantAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.CameraFilterVariant;

        public CameraFilterVariantAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var cameraFilterVariant = ev.GetCameraFilterVariant();
            if (cameraFilterVariant == null) return Empty;
            
            var loadedVariants = AssetManager.GetAllLoadedAssets(DbModelType.CameraFilterVariant);
            return loadedVariants.Where(x => cameraFilterVariant.Id == x.Id).ToArray();
        }
    }
}
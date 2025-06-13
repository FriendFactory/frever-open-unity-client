using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class SetLocationAssetProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.SetLocation;
        
        public SetLocationAssetProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var setLocationId = ev.GetSetLocationId();

            var loadedAssets = AssetManager.GetAllLoadedAssets(DbModelType.SetLocation);
            return loadedAssets.Where(x => x.Id == setLocationId).ToArray();
        }
    }
}
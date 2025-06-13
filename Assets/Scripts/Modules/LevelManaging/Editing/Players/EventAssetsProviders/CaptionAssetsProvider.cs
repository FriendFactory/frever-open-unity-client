using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class CaptionAssetsProvider : EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.Caption;
        
        public CaptionAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            if (ev.Caption == null) return Empty;

            var allCaptions = AssetManager.GetAllLoadedAssets(DbModelType.Caption);
            return allCaptions.Where(x => ev.Caption.Any(_=>_.Id == x.Id)).ToArray();
        }
    }
}
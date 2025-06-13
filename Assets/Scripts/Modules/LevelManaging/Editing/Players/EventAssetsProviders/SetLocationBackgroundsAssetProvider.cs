using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class SetLocationBackgroundsAssetProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.SetLocationBackground;
        
        public SetLocationBackgroundsAssetProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var background = ev.GetFreverBackground();
            if (background == null) return Empty;

            var allBackgrounds = AssetManager.GetAllLoadedAssets(DbModelType.SetLocationBackground);
            return allBackgrounds.Where(x => x.Id == background.Id).ToArray();
        }
    }
}
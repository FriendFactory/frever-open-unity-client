using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class BodyAnimationAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.BodyAnimation;

        public BodyAnimationAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var bodyAnimIds = ev.GetUniqueBodyAnimationIds();

            var bodyAnimAssets = AssetManager.GetAllLoadedAssets(DbModelType.BodyAnimation);
            return bodyAnimAssets.Where(x => bodyAnimIds.Contains(x.Id)).ToArray();
        }
    }
}
using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class VfxAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.Vfx;
        
        public VfxAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        public override IAsset[] GetLoadedAssets(Event ev)
        {
            if (!ev.HasVfx()) return Empty;

            var vfxId = ev.GetVfx().Id;

            var loadedVfx = AssetManager.GetAllLoadedAssets(DbModelType.Vfx);
            return loadedVfx.Where(x => vfxId == x.Id).ToArray();
        }
    }
}
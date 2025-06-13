using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class ExternalTrackAssetsProvider : EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.ExternalTrack;
        
        public ExternalTrackAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var externalTrack = ev.GetExternalTrack();
            if (externalTrack == null) return Empty;
            
            var externalSong = AssetManager.GetAllLoadedAssets(DbModelType.ExternalTrack);
            return externalSong.Where(x => x.Id == externalTrack.Id).ToArray();
        }
    }
}
using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class SongAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.Song;
        
        public SongAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var song = ev.GetSong();
            if (song == null) return Empty;
            
            var songAssets = AssetManager.GetAllLoadedAssets(DbModelType.Song);
            return songAssets.Where(x => x.Id == song.Id).ToArray();
        }
    }
}
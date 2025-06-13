using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal sealed class VideoAssetsProvider: EventAssetsProviderBase
    {
        public override DbModelType TargetType => DbModelType.VideoClip;
        
        public VideoAssetsProvider(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override IAsset[] GetLoadedAssets(Event ev)
        {
            var videoClip = ev.GetVideo();
            if (videoClip == null) return Empty;

            var allVideo = AssetManager.GetAllLoadedAssets(DbModelType.VideoClip);
            return allVideo.Where(x => x.Id == videoClip.Id).ToArray();
        }
    }
}
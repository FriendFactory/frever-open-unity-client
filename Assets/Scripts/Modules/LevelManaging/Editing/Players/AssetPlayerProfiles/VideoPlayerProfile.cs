using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class VideoPlayerProfile: GenericAssetPlayerProfile<IVideoClipAsset, VideoAssetPlayer, VideoAssetPlayerSetup, VideoAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.VideoClip;
        
        public VideoPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }

        protected override VideoAssetPlayer CreatePlayer()
        {
            return new VideoAssetPlayer();
        }

        protected override VideoAssetPlayerSetup CreateSetup()
        {
            return new VideoAssetPlayerSetup(AssetManager);
        }

        protected override VideoAssetsProvider CreateEventAssetsProvider()
        {
            return new VideoAssetsProvider(AssetManager);
        }
    }
}
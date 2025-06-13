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
    internal sealed class PhotoPlayerProfile: GenericAssetPlayerProfile<IPhotoAsset, PhotoAssetPlayer, PhotoPlayerSetup, PhotoAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.UserPhoto;

        public PhotoPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }

        protected override PhotoAssetPlayer CreatePlayer()
        {
            return new PhotoAssetPlayer();
        }

        protected override PhotoPlayerSetup CreateSetup()
        {
            return new PhotoPlayerSetup(AssetManager);
        }

        protected override PhotoAssetsProvider CreateEventAssetsProvider()
        {
            return new PhotoAssetsProvider(AssetManager);
        }
    }
}
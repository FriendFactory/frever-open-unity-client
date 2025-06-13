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
    internal sealed class UserSoundPlayerProfile: GenericAssetPlayerProfile<IUserSoundAsset, UserSoundAssetPlayer, UserSoundPlayerSetup, UserSoundAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.UserSound;
        
        public UserSoundPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        protected override UserSoundAssetPlayer CreatePlayer()
        {
            return new UserSoundAssetPlayer();
        }

        protected override UserSoundPlayerSetup CreateSetup()
        {
            return new UserSoundPlayerSetup();
        }

        protected override UserSoundAssetsProvider CreateEventAssetsProvider()
        {
            return new UserSoundAssetsProvider(AssetManager);
        }
    }
}
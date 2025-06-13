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
    internal sealed class CaptionPlayerProfile: GenericAssetPlayerProfile<ICaptionAsset, CaptionAssetPlayer, CaptionPlayerSetup, CaptionAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.Caption;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CaptionPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override CaptionAssetPlayer CreatePlayer()
        {
            return new CaptionAssetPlayer();
        }

        protected override CaptionPlayerSetup CreateSetup()
        {
            return new CaptionPlayerSetup();
        }

        protected override CaptionAssetsProvider CreateEventAssetsProvider()
        {
            return new CaptionAssetsProvider(AssetManager);
        }
    }
}
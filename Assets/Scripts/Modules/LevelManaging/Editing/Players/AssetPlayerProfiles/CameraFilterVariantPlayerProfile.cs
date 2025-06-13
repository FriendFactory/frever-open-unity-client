using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    internal sealed class CameraFilterVariantPlayerProfile :
        GenericAssetPlayerProfile<ICameraFilterVariantAsset, CameraFilterVariantPlayer, CameraFilterVariantPlayerSetup, CameraFilterVariantAssetsProvider>
    {
        public CameraFilterVariantPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType AssetType => DbModelType.CameraFilterVariant;

        protected override CameraFilterVariantPlayer CreatePlayer()
        {
            return new CameraFilterVariantPlayer();
        }

        protected override CameraFilterVariantPlayerSetup CreateSetup()
        {
            return new CameraFilterVariantPlayerSetup();
        }

        protected override CameraFilterVariantAssetsProvider CreateEventAssetsProvider()
        {
            return new CameraFilterVariantAssetsProvider(AssetManager);
        }
    }
}
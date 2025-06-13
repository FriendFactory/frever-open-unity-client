using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class CameraAnimationPlayerProfile: GenericAssetPlayerProfile<ICameraAnimationAsset, CameraAnimationPlayer, CameraAnimationPlayerSetup, CameraAnimationAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.CameraAnimation;

        private readonly ICameraSystem _cameraSystem;

        public CameraAnimationPlayerProfile(ICameraSystem cameraSystem, IAssetManager assetManager): base(assetManager)
        {
            _cameraSystem = cameraSystem;
        }

        protected override CameraAnimationPlayer CreatePlayer()
        {
            return new CameraAnimationPlayer(_cameraSystem);
        }

        protected override CameraAnimationPlayerSetup CreateSetup()
        {
            return new CameraAnimationPlayerSetup();
        }

        protected override CameraAnimationAssetsProvider CreateEventAssetsProvider()
        {
            return new CameraAnimationAssetsProvider(AssetManager);
        }
    }
}
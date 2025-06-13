using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class VfxPlayerProfile: GenericAssetPlayerProfile<IVfxAsset, VfxAssetPlayer, VfxPlayerSetup, VfxAssetsProvider>
    {
        private readonly AnimatorMonitorProvider _animatorMonitorProvider;
        
        public override DbModelType AssetType => DbModelType.Vfx;
        
        public VfxPlayerProfile(IAssetManager assetManager, AnimatorMonitorProvider animatorMonitorProvider) : base(assetManager)
        {
            _animatorMonitorProvider = animatorMonitorProvider;
        }

        protected override VfxAssetPlayer CreatePlayer()
        {
            return new VfxAssetPlayer(_animatorMonitorProvider);
        }

        protected override VfxPlayerSetup CreateSetup()
        {
            return new VfxPlayerSetup();
        }

        protected override VfxAssetsProvider CreateEventAssetsProvider()
        {
            return new VfxAssetsProvider(AssetManager);
        }
    }
}
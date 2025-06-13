using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;
using Zenject;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class BodyAnimationPlayerProfile: GenericAssetPlayerProfile<IBodyAnimationAsset, BodyAnimationPlayer, BodyAnimationPlayerSetup, BodyAnimationAssetsProvider>
    {
        private readonly DiContainer _diContainer;
        private readonly IDataFetcher _dataFetcher;
        private readonly AnimatorMonitorProvider _animatorMonitorProvider;

        public override DbModelType AssetType => DbModelType.BodyAnimation;

        public BodyAnimationPlayerProfile(IAssetManager assetManager, DiContainer diContainer, IDataFetcher dataFetcher, AnimatorMonitorProvider animatorMonitorProvider) : base(assetManager)
        {
            _diContainer = diContainer;
            _dataFetcher = dataFetcher;
            _animatorMonitorProvider = animatorMonitorProvider;
        }

        protected override BodyAnimationPlayer CreatePlayer()
        {
            return new BodyAnimationPlayer(_dataFetcher.MetadataStartPack.MovementTypes, _diContainer.Resolve<IEventEditor>(), _animatorMonitorProvider);
        }

        protected override BodyAnimationPlayerSetup CreateSetup()
        {
            return new BodyAnimationPlayerSetup(_diContainer.Resolve<EventAssetsProvider>());
        }

        protected override BodyAnimationAssetsProvider CreateEventAssetsProvider()
        {
            return new BodyAnimationAssetsProvider(AssetManager);
        }
    }
}
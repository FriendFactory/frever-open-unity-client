using Common.TimeManaging;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class FaceAnimationPlayerProfile: GenericAssetPlayerProfile<IFaceAnimationAsset, FaceAnimationAssetPlayer, FaceAnimationPlayerSetup, FaceAnimationAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.FaceAnimation;

        private readonly AudioSourceManager _audioSourceManager;
        private readonly IAssetManager _assetManager;
        private readonly IUnityTimeBasedTimeSource _unityTimeBasedTimeSource;
        private readonly IAudioBasedTimeSource _audioBasedTimeSource;

        public FaceAnimationPlayerProfile(IAssetManager assetManager, AudioSourceManager audioSourceManager, IUnityTimeBasedTimeSource unityTimeBasedTimeSource, IAudioBasedTimeSource audioBasedTimeSource) : base(assetManager)
        {
            _audioSourceManager = audioSourceManager;
            _assetManager = assetManager;
            _unityTimeBasedTimeSource = unityTimeBasedTimeSource;
            _audioBasedTimeSource = audioBasedTimeSource;
        }

        protected override FaceAnimationAssetPlayer CreatePlayer()
        {
            return new FaceAnimationAssetPlayer();
        }

        protected override FaceAnimationPlayerSetup CreateSetup()
        {
            return new FaceAnimationPlayerSetup(_assetManager, _audioSourceManager, _unityTimeBasedTimeSource, _audioBasedTimeSource);
        }

        protected override FaceAnimationAssetsProvider CreateEventAssetsProvider()
        {
            return new FaceAnimationAssetsProvider(AssetManager);
        }
    }
}
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;
using Modules.CameraSystem.CameraAnimations;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class CameraAnimationLoadProfile: AssetLoadProfile<CameraAnimationFullInfo, CameraAnimLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly CameraAnimationConverter _cameraAnimationConverter;

        public CameraAnimationLoadProfile(IBridge bridge, CameraAnimationConverter cameraAnimationConverter)
        {
            _bridge = bridge;
            _cameraAnimationConverter = cameraAnimationConverter;
        }

        public override AssetLoader<CameraAnimationFullInfo, CameraAnimLoadArgs> GetAssetLoader()
        {
            return new CameraAnimationLoader(_bridge, _cameraAnimationConverter);
        }

        public override AssetUnloader GetUnloader()
        {
            return new CameraAnimationUnloader();
        }
    }
}
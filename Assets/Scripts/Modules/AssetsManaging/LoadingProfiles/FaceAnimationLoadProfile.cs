using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;
using Modules.FaceAndVoice.Face.Recording.Core;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class FaceAnimationLoadProfile: AssetLoadProfile<FaceAnimationFullInfo, FaceAnimationLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly FaceAnimationConverter _faceAnimationConverter;

        public FaceAnimationLoadProfile(IBridge bridge, FaceAnimationConverter faceAnimationConverter)
        {
            _bridge = bridge;
            _faceAnimationConverter = faceAnimationConverter;
        }

        public override AssetLoader<FaceAnimationFullInfo, FaceAnimationLoadArgs> GetAssetLoader()
        {
            return new FaceAnimationLoader(_bridge, _faceAnimationConverter);
        }

        public override AssetUnloader GetUnloader()
        {
            return new FaceAnimationUnloader();
        }
    }
}
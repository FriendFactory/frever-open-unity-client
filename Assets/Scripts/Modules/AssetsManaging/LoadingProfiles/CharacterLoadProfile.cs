using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Loaders.DependencyLoading;
using Modules.AssetsManaging.Unloaders;
using Modules.AssetsStoraging.Core;
using Modules.FaceAndVoice.Face.Core;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class CharacterLoadProfile: AssetLoadProfile<CharacterFullInfo, CharacterLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly CharacterViewContainer _viewContainer;
        private readonly FaceBlendShapeMap _faceBlendShapeMap;

        public CharacterLoadProfile(IBridge bridge, CharacterViewContainer viewContainer, FaceBlendShapeMap faceBlendShapeMap)
        {
            _bridge = bridge;
            _viewContainer = viewContainer;
            _faceBlendShapeMap = faceBlendShapeMap;
        }

        public override AssetLoader<CharacterFullInfo, CharacterLoadArgs> GetAssetLoader()
        {
            return new CharacterLoader(_bridge, _viewContainer, _faceBlendShapeMap);
        }

        public override AssetUnloader GetUnloader()
        {
            return new CharacterUnloader(_viewContainer);
        }

        public override DependencyLoader<CharacterFullInfo, CharacterLoadArgs> GetDependencyLoader()
        {
            return new CharacterDependenciesLoader(_viewContainer);
        }
    }
}
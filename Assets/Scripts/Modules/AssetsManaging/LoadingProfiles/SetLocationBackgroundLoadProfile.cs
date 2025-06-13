using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class SetLocationBackgroundLoadProfile : AssetLoadProfile<SetLocationBackground, SetLocationBackgroundLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly ISetLocationBackgroundInMemoryCache _setLocationBackgroundInMemoryCache;

        public SetLocationBackgroundLoadProfile(IBridge bridge, ISetLocationBackgroundInMemoryCache setLocationBackgroundInMemoryCache)
        {
            _bridge = bridge;
            _setLocationBackgroundInMemoryCache = setLocationBackgroundInMemoryCache;
        }

        public override AssetLoader<SetLocationBackground, SetLocationBackgroundLoadArgs> GetAssetLoader()
        {
            return new SetLocationBackgroundLoader(_bridge, _setLocationBackgroundInMemoryCache);
        }

        public override AssetUnloader GetUnloader()
        {
            return new SetLocationBackgroundUnloader(_setLocationBackgroundInMemoryCache);
        }
    }
}
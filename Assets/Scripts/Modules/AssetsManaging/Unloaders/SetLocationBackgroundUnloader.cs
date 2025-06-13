using System;
using Modules.AssetsManaging.Loaders;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class SetLocationBackgroundUnloader : TextureBaseAssetUnloader
    {
        private readonly ISetLocationBackgroundInMemoryCache _inMemoryCache;

        public SetLocationBackgroundUnloader(ISetLocationBackgroundInMemoryCache inMemoryCache)
        {
            _inMemoryCache = inMemoryCache;
        }

        public override void Unload(IAsset asset, Action onSuccess)
        {
            var backgroundAsset = asset as ISetLocationBackgroundAsset;
            _inMemoryCache.Add(asset.Id, backgroundAsset.Texture);
        }
    }
}
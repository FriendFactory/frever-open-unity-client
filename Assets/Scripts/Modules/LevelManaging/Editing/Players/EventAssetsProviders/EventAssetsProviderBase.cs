using System;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal abstract class EventAssetsProviderBase: IEventAssetsProvider
    {
        public abstract DbModelType TargetType { get; }
        public abstract IAsset[] GetLoadedAssets(Event ev);

        protected readonly IAssetManager AssetManager;
        
        protected static readonly IAsset[] Empty = Array.Empty<IAsset>();

        protected EventAssetsProviderBase(IAssetManager assetManager)
        {
            AssetManager = assetManager;
        }
    }
}
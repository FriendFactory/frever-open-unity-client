using System.Collections.Generic;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing
{
    // Extract loaded asset for particular event
    [UsedImplicitly]
    internal sealed class EventAssetsProvider
    {
        private readonly IProviderAssetProviders _providersSource;

        public EventAssetsProvider(IProviderAssetProviders providersSource)
        {
            _providersSource = providersSource;
        }

        public IAsset[] GetLoadedAssets(Event ev, params DbModelType[] targetTypes)
        {
            var output = new List<IAsset>();
            
            foreach (var type in targetTypes)
            {
                var assetsProvider = _providersSource.GetAssetsProvider(type);
                var assets = assetsProvider.GetLoadedAssets(ev);
                output.AddRange(assets);
            }

            return output.ToArray();
        }
    }
}
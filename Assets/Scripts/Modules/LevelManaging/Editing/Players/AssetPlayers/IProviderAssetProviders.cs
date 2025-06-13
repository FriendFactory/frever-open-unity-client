using Extensions;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal interface IProviderAssetProviders
    {
        IEventAssetsProvider GetAssetsProvider(DbModelType targetType);
    }
}
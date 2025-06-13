using Extensions;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    /// <summary>
    ///     Responsible for providing all necessary services required for asset playing
    /// </summary>
    internal interface IAssetPlayerProfile
    {
        DbModelType AssetType { get; }
        IAssetPlayer GetPlayer();
        IPlayerSetup GetPlayerSetup();
        IEventAssetsProvider GetAssetsProvider();
    }

    /// <summary>
    /// Simplify injection profiles per asset type
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    internal interface IAssetPlayerProfile<TAsset> : IAssetPlayerProfile where TAsset: IAsset
    {
    }
}
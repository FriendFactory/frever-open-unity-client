using Extensions;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public interface IAssetPlayersProvider: IPlayableTypesProvider
    {
        bool IsSupported(DbModelType assetType);
        IAssetPlayer CreateAssetPlayer(IAsset targetAsset);
    }
}
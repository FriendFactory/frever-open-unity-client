using Models;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal interface IPlayerSetup<TPlayer>: IPlayerSetup where TPlayer: IAssetPlayer
    {
        
    }
    
    internal interface IPlayerSetup
    { 
        void Setup(IAssetPlayer assetPlayer, Event ev);
    }
}
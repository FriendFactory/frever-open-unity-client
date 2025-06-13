using System.Collections.Generic;
using Extensions;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    /// <summary>
    /// Responsible for providing players and their setups 
    /// </summary>
    internal interface IPlayersManager: IAssetPlayersProvider, IPlayerSetupProvider, IProviderAssetProviders
    {
        IReadOnlyCollection<DbModelType> AudioTypes { get; }
    }
}
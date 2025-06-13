using System.Collections.Generic;
using Extensions;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public interface IPlayableTypesProvider
    {
        IReadOnlyCollection<DbModelType> PlayableTypes { get; }
    }
}
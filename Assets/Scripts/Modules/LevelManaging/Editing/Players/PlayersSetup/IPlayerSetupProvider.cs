using Extensions;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal interface IPlayerSetupProvider
    {
        IPlayerSetup GetSetup(DbModelType type);
    }
}
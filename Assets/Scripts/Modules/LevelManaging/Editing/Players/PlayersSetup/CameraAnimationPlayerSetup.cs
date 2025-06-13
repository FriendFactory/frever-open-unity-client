using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class CameraAnimationPlayerSetup: GenericSetup<ICameraAnimationAsset, CameraAnimationPlayer>
    {
        protected override void SetupPlayer(CameraAnimationPlayer player, Event ev)
        {
            //player does not require any setup
        }
    }
}
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class CameraFilterVariantPlayerSetup: GenericSetup<ICameraFilterVariantAsset, CameraFilterVariantPlayer>
    {
        protected override void SetupPlayer(CameraFilterVariantPlayer player, Event ev)
        {
        }
    }
}
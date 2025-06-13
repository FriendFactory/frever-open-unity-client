using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class CaptionPlayerSetup : GenericSetup<ICaptionAsset, CaptionAssetPlayer>
    {
        protected override void SetupPlayer(CaptionAssetPlayer player, Event ev)
        {
        }
    }
}
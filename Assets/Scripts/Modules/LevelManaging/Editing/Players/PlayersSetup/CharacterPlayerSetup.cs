using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class CharacterPlayerSetup: GenericSetup<ICharacterAsset, CharacterPlayer>
    {
        protected override void SetupPlayer(CharacterPlayer player, Event ev)
        {
            player.PrepareCharacter(ev);
        }
    }
}
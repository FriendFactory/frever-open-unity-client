using System.Linq;
using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UnityEngine;
using CharacterController = Models.CharacterController;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class BodyAnimationPlayerSetup: GenericSetup<IBodyAnimationAsset, BodyAnimationPlayer>
    {
        private readonly EventAssetsProvider _eventAssetsProvider;

        public BodyAnimationPlayerSetup(EventAssetsProvider eventAssetsProvider)
        {
            _eventAssetsProvider = eventAssetsProvider;
        }

        protected override void SetupPlayer(BodyAnimationPlayer player, Event ev)
        {
            PrepareStartTimeForPlayer(player, ev);
            PrepareCharactersForPlayer(player, ev);
        }

        private void PrepareStartTimeForPlayer(BodyAnimationPlayer player, Event ev)
        {
            var bodyAnimController = ev.GetCharacterBodyAnimationControllersWithAnimationId(player.AssetId).First();
            var activationCue = bodyAnimController.ActivationCue.ToKilo();
            player.SetStartTime(activationCue); 
        } 
        
        private void PrepareCharactersForPlayer(BodyAnimationPlayer player, Event ev)
        {
            var playerCharacterControllers = ev.GetCharacterControllersWithBodyAnimationId(player.AssetId);
            var usedByCharacters = playerCharacterControllers.Select(x => x.CharacterId);
            var characters = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.Character)
                                                 .Where(x=>usedByCharacters.Contains(x.Id)).Cast<ICharacterAsset>().ToArray();
            player.SetCharacters(characters, playerCharacterControllers);
            player.SetSpawnPositionTransform(GetCharacterSpawnPositionTransform(ev));
            player.SetCurrentlyActiveCharacterCount(characters.Length);
        }

        private Transform GetCharacterSpawnPositionTransform(Event ev)
        {
            var setLocation = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.SetLocation)
                                                  .Cast<ISetLocationAsset>().First();
            var spawnPositionGuid = ev.CurrentCharacterSpawnPosition().UnityGuid;
            var spawnPositionTransform = setLocation.GetCharacterSpawnPositionTransform(spawnPositionGuid);
            return spawnPositionTransform;
        }
    }
}
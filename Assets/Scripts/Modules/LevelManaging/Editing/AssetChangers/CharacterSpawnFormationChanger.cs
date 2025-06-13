using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using CharacterController = Models.CharacterController;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class CharacterSpawnFormationChanger
    {
        private readonly EventAssetsProvider _eventAssetsProvider;
        private readonly IFormation[] _formations;

        public CharacterSpawnFormationChanger(EventAssetsProvider eventAssetsProvider, IFormation[] formations)
        {
            _eventAssetsProvider = eventAssetsProvider;
            _formations = formations;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Run(long? spawnFormationId, Event ev)
        {
            ev.CharacterSpawnPositionFormationId = spawnFormationId;
            var characterAssets = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.Character)
                .Cast<ICharacterAsset>().ToArray();
            var charactersIds = characterAssets.Select(c => c.Id).ToArray();
            var characterControllers = ev.GetCharacterControllersByCharactersIds(charactersIds);
            var loadedAssets = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.SetLocation);
            if (loadedAssets.Length == 0) 
                return;
            var setLocationAsset = loadedAssets.First() as ISetLocationAsset;
            var spawnPosition = ev.CurrentCharacterSpawnPosition();

            Run(spawnFormationId, characterAssets, characterControllers, setLocationAsset, spawnPosition);
        }

        public void Run(long? spawnFormationId, ICharacterAsset[] characters, CharacterController[] characterControllers, ISetLocationAsset setLocation, CharacterSpawnPositionInfo spawnPosition)
        {
            var orderedCharacterControllers = characterControllers.OrderBy(x => x.ControllerSequenceNumber).ToArray();

            if (orderedCharacterControllers.Length == 0) return;
            
            var orderedCharacters = new ICharacterAsset[characters.Length];
            
            for (var i = 0; i < orderedCharacterControllers.Length; i++)
            {
                var character = characters.FirstOrDefault(c => c.Id == orderedCharacterControllers[i].CharacterId);
                orderedCharacters[i] = character;
            }
            
            AttachToTheSameSpawnPosition(characters, characterControllers, setLocation, spawnPosition);

            if (characters.Length > 1)
            {
                var spawnPoint = setLocation.GetCharacterSpawnPositionTransform(spawnPosition.UnityGuid);
                var targetFormation = _formations.First(x => x.Id == spawnFormationId);
                targetFormation.Run(orderedCharacters, spawnPoint);
            }

            foreach (var character in orderedCharacters)
            {
                character.ResetHairPosition();
            }
        }

        private static void AttachToTheSameSpawnPosition(ICharacterAsset[] characters,
            CharacterController[] characterControllers, ISetLocationAsset setLocation, CharacterSpawnPositionInfo spawnPosition)
        {
            setLocation.Attach(spawnPosition, characters);
            setLocation.ResetPosition(characters);
            foreach (var controller in characterControllers)
            {
                controller.CharacterSpawnPositionId = spawnPosition.Id;
            }
        }
    }
}
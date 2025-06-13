using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;

namespace Modules.LevelManaging.Editing
{
    [UsedImplicitly]
    internal sealed class CharacterRemovingAlgorithm
    {
        private readonly CharactersChanger _charactersChanger;
        private readonly IAssetManager _assetManager;
        private readonly IAnimationGroupProvider _animationGroupProvider;
        private readonly CharacterSpawnFormationChanger _characterSpawnFormationChanger;
        private readonly DefaultBodyAnimationForSpawnPositionLoader _defaultBodyAnimationForSpawnPositionLoader;
        private readonly SpawnFormationProvider _spawnFormationProvider;

        public CharacterRemovingAlgorithm(CharactersChanger charactersChanger, IAssetManager assetManager, IAnimationGroupProvider animationGroupProvider, CharacterSpawnFormationChanger characterSpawnFormationChanger, DefaultBodyAnimationForSpawnPositionLoader defaultBodyAnimationForSpawnPositionLoader, SpawnFormationProvider spawnFormationProvider)
        {
            _charactersChanger = charactersChanger;
            _assetManager = assetManager;
            _animationGroupProvider = animationGroupProvider;
            _characterSpawnFormationChanger = characterSpawnFormationChanger;
            _defaultBodyAnimationForSpawnPositionLoader = defaultBodyAnimationForSpawnPositionLoader;
            _spawnFormationProvider = spawnFormationProvider;
        }

        public void Run(Event targetEvent, CharacterFullInfo character, Action callback)
        {
            if (targetEvent.CharacterController.Count <= 1) return;
            
            UnloadCharacterRelatedAssetsIfNecessary(targetEvent, character.Id);

            async void OnSuccess()
            {
                await OnCharacterDespawn(targetEvent);
                callback?.Invoke();
            }

            _charactersChanger.DespawnCharacter(character, targetEvent, OnSuccess);
        }

        private async Task OnCharacterDespawn(Event targetEvent)
        {
            SetDefaultSpawnFormation(targetEvent);
                
            var isSingleCharacterWithMultiCharacterAnimation = targetEvent.CharactersCount() == 1 &&
                                                               targetEvent.GetTargetCharacterController()
                                                                          .GetBodyAnimation().IsMultiCharacter();
                
            if (targetEvent.HasSetupMultiCharacterAnimation() || isSingleCharacterWithMultiCharacterAnimation)
            {
                var groupId = targetEvent.GetMultiCharacterAnimationGroupId();
                var animationGroup = await _animationGroupProvider.GetAnimationGroup(groupId);
                if (animationGroup.Length != targetEvent.GetCharactersCount())
                {
                    await _defaultBodyAnimationForSpawnPositionLoader.ApplyDefaultBodyAnimationForAllCharacters(targetEvent);
                }
            }

            if (targetEvent.AreCharactersOnTheSameSpawnPosition())
            {
                _characterSpawnFormationChanger.Run(targetEvent.CharacterSpawnPositionFormationId, targetEvent);
            }
        }
        
        private void UnloadCharacterRelatedAssetsIfNecessary(Event ev, long characterId)
        {
            var controllers = ev.CharacterController;
            var charToBeRemoved = controllers.FirstOrDefault(cc => cc.Character.Id == characterId);

            var bodyAnimationId = charToBeRemoved.GetBodyAnimationId();
            var otherCharacterUsingSameBodyAnim = controllers.Where(x => x != charToBeRemoved)
                                                             .Any(controller => controller.GetBodyAnimationId() == bodyAnimationId);

            if (otherCharacterUsingSameBodyAnim) return;
            var bodyAnim = _assetManager.GetActiveAssets<IBodyAnimationAsset>()
                                       ?.FirstOrDefault(asset => asset.Id == bodyAnimationId);
            _assetManager.Unload(bodyAnim);
        }
        
        private void SetDefaultSpawnFormation(Event ev)
        {
            ev.CharacterSpawnPositionFormationId = _spawnFormationProvider.GetDefaultSpawnFormationId(ev);
        }
    }
}
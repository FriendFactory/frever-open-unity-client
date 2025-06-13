using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsStoraging.Core;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    public interface ISpawnFormationProvider
    {
        int MaxCharacterCount { get; }
        long GetDefaultSpawnFormationId(Event ev);
        CharacterSpawnPositionFormation[] GetSpawnPositionFormations(int characterCount);
    }

    [UsedImplicitly]
    internal sealed class SpawnFormationProvider: ISpawnFormationProvider
    {
        private readonly IDataFetcher _dataFetcher;

        private ICollection<CharacterSpawnPositionFormation> AllSpawnFormations =>
            _dataFetcher.CharacterSpawnPositionFormations;

        public int MaxCharacterCount => AllSpawnFormations.Max(x => x.CharacterCount);
        
        public SpawnFormationProvider(IDataFetcher dataFetcher)
        {
            _dataFetcher = dataFetcher;
        }

        public long GetDefaultSpawnFormationId(Event ev)
        {
            if (ev.HasSetupMultiCharacterAnimation())
            {
                return GetDefaultMultiCharacterAnimationSpawnFormationId(ev);
            }
            
            var characterCount = ev.CharacterController.Count;
            return GetDefaultSpawnFormation(characterCount).Id;
        }

        public CharacterSpawnPositionFormation[] GetSpawnPositionFormations(int characterCount)
        {
            return AllSpawnFormations.Where(x => x.CharacterCount == characterCount).ToArray();
        }
        
        public CharacterSpawnPositionFormation GetSpawnPositionFormation(long id)
        {
            return AllSpawnFormations.FirstOrDefault(x=>x.Id == id);
        }

        private CharacterSpawnPositionFormation GetDefaultSpawnFormation(int charactersCount)
        {
            Validate(charactersCount);
            return GetSpawnPositionFormations(charactersCount).OrderBy(x => x.SortOrder).First();
        }
        
        private long GetDefaultMultiCharacterAnimationSpawnFormationId(Event ev)
        {
            var characterCount = ev.GetCharactersCount();
            //todo: remove hardcoded "Trio line", FREV-14147
            return characterCount == 3 ? AllSpawnFormations.First(x => x.Name == "Trio line").Id : GetDefaultSpawnFormation(characterCount).Id;
        }

        private void Validate(int charactersCount)
        {
            if (charactersCount > MaxCharacterCount)
                throw new InvalidOperationException(
                    $"Can't provide spawn formation for characters count {charactersCount}. " +
                    $"Reason: max allowed count is {MaxCharacterCount}");
        }
    }
}
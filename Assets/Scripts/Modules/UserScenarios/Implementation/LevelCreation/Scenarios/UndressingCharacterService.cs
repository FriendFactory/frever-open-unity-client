using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.LocalStorage;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    internal interface IUndressingCharacterService
    {
        OutfitFullInfo CreateUndressedOutfit(CharacterFullInfo character);
    }
    
    [UsedImplicitly]
    internal sealed class UndressingCharacterService: IUndressingCharacterService
    {
        private readonly IMetadataProvider _metadataProvider;
        private readonly CharacterOutfitDefaultDataProvider _characterOutfitDefaultDataProvider;
        
        private static Texture2D _fakeTexture;
        private long[] _keepSubCategories;

        private long[] KeepSubCategories
        {
            get
            {
                _keepSubCategories ??= _metadataProvider
                                          .WardrobeCategories.SelectMany(x => x.SubCategories)
                                          .Where(x => x.KeepOnUndressing).Select(x => x.Id)
                                          .ToArray();
                
                return _keepSubCategories;
            }
        }
        
        public UndressingCharacterService(IMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
            _characterOutfitDefaultDataProvider = new CharacterOutfitDefaultDataProvider();
        }

        public OutfitFullInfo CreateUndressedOutfit(CharacterFullInfo character)
        {
            if (_fakeTexture == null)
            {
                _fakeTexture = new Texture2D(4, 4);
            }

            var wardrobes = character.Wardrobes?
                    .Where(x => x.SubCategories is { Length: > 0 } &&
                                x.SubCategories.Any(subCategoryId => KeepSubCategories.Contains(subCategoryId)))
                    .ToList();

            // In case character is already naked
            wardrobes ??= new List<WardrobeFullInfo>();

            return new OutfitFullInfo
            {
                Id = LocalStorageManager.GetNextLocalId(nameof(OutfitFullInfo)),
                Wardrobes = wardrobes,
                SaveMethod = SaveOutfitMethod.Automatic,
                GenderWardrobes = new Dictionary<long, List<long>>
                {
                    {character.GenderId, wardrobes.Select(x=>x.Id).ToList()}
                },
                Files = _characterOutfitDefaultDataProvider.GetDefaultFiles(),
            };
        }
    }
}
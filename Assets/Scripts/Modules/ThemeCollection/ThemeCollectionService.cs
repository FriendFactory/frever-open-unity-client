using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.ThemeCollection;
using Common;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using Zenject;
using Extensions;

namespace Modules.ThemeCollection
{
    [UsedImplicitly]
    public sealed class ThemeCollectionService : IThemeCollectionService
    {
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IMetadataProvider _metadataProvider;

        public IEnumerable<ThemeCollectionInfo> ThemeCollections => GetCurrentThemeCollections();
        
        public async void ShowCollection(long? id = null)
        {
            var fullCharacterResp = await _characterManager.GetCharacterAsync(_characterManager.SelectedCharacter.Id);
            _scenarioManager.ExecuteCharacterEditing(fullCharacterResp, Constants.Wardrobes.CLOTHES_CATEGORY_TYPE_ID, id);
        }

        private IEnumerable<ThemeCollectionInfo>  GetCurrentThemeCollections()
        {
            var raceId = _characterManager.RaceMainCharacters
                .First(c => c.Value == _characterManager.SelectedCharacter.Id).Key;
            var universeId = _metadataProvider.MetadataStartPack.GetUniverseByRaceId(raceId).Id;
            return _dataFetcher.MetadataStartPack.ThemeCollections.Where(c => c.UniverseId == universeId);
        }
    }
}
using System;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Modules.UniverseManaging
{
    public interface IUniverseManager
    {
        Universe LastSelectedUniverse { get; }
        
        void SelectUniverse(Action<Universe> selected, Action userCancelled = null);
        bool TryShowEndSeasonPopup();
    }
    
    [UsedImplicitly]
    internal sealed class UniverseManager: IUniverseManager
    {
        private readonly PopupManager _popupManager;
        private readonly IMetadataProvider _metadataProvider;
        private readonly CharacterManager _characterManager;
        private readonly IScenarioManager _scenarioManager;

        public Universe LastSelectedUniverse { get; private set; }

        public UniverseManager(PopupManager popupManager, IMetadataProvider metadataProvider, CharacterManager characterManager, IScenarioManager scenarioManager)
        {
            _popupManager = popupManager;
            _metadataProvider = metadataProvider;
            _characterManager = characterManager;
            _scenarioManager = scenarioManager;
        }

        public void SelectUniverse(Action<Universe> selected, Action userCancelled)
        {
            var selectedCallback = new Action<Universe>(universe =>
            {
                LastSelectedUniverse = universe;
                selected?.Invoke(universe);
            });
            
            var activeUniverses = _metadataProvider.MetadataStartPack.GetActiveUniverses().ToArray();
            if (activeUniverses.Length == 1)
            {
                selectedCallback.Invoke(activeUniverses.First());
            }
            else
            {
                _popupManager.SetupPopup(new IPSelectionPopupConfiguration(_metadataProvider.MetadataStartPack.Universes, selectedCallback, userCancelled));
                _popupManager.ShowPopup(PopupType.IPSelection);
            }
        }

        public bool TryShowEndSeasonPopup()
        {
            var activeUniverses = _metadataProvider.MetadataStartPack.GetActiveUniverses().ToArray();
            if (activeUniverses.Length > 1)
            {
                return false;
            }

            var universe = activeUniverses[0];
            var hasFreverCharacter = _characterManager.UserCharacters.Any(character =>
                _metadataProvider.MetadataStartPack.GetUniverseByGenderId(character.GenderId)?.Id == universe.Id);
            if (hasFreverCharacter)
            {
                return false;
            }

            var config = new SeasonEndedPopupConfiguration
            {
                OnButtonClick = () => _scenarioManager.ExecuteNewCharacterCreation(
                    universe.Races[0].RaceId,
                    () => _popupManager.ClosePopupByType(PopupType.SeasonEndedPopup)
                ),
                ButtonText = "Create New Character"
            };

            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal class CharacterSelectionState: StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly PopupManager _popupManager;
        private readonly CharacterManager _characterManager;
        private readonly PageManager _pageManager;
        private readonly InformationPopupConfiguration _loadingPopupConfiguration;
        private readonly IMetadataProvider _metadataProvider;
        
        public ITransition MoveNext;
        public ITransition MoveBack;
        
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        
        public override ScenarioState Type => ScenarioState.CharacterSelection;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();

        public CharacterSelectionState(PopupManager popupManager, CharacterManager characterManager, PageManager pageManager, IMetadataProvider metadataProvider)
        {
            ProjectContext.Instance.Container.Inject(this);
            
            _popupManager = popupManager;
            _characterManager = characterManager;
            _pageManager = pageManager;
            _metadataProvider = metadataProvider;

            _loadingPopupConfiguration = new InformationPopupConfiguration
            {
                PopupType = PopupType.Loading,
                Title = _loadingOverlayLocalization.SettingTheStageHeader
            };
        }

        public override void Run()
        {
            var presetIds = (Context.CharacterSelection.AutoPickedCharacters == null) ? null : new HashSet<long>(Context.CharacterSelection.AutoPickedCharacters);
            var characterToReplace = Context.CharacterSelection.CharacterToReplaceIds;
            var allCharacters = presetIds != null ? new HashSet<long>(presetIds.Concat(characterToReplace)): new HashSet<long>(characterToReplace);
            var universeId = _metadataProvider.MetadataStartPack
                                              .GetUniverseByRaceId(Context.CharacterSelection.Race.Id).Id;//todo: change to dynamic based on extra step
            var popupConfig = new CharacterSelectionPopupConfiguration(allCharacters, presetIds,
                Context.CharacterSelection.ReasonText,
                Context.CharacterSelection.Header,
                Context.CharacterSelection.HeaderDescription,
                universeId,
                OnCharactersSelected,
                OnCancelSelection);
            _popupManager.ClosePopupByType(PopupType.Loading);
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(PopupType.CharacterSelection);
        }

        protected async void OnCharactersSelected(Dictionary<long, CharacterInfo> characters)
        {
            var ids = characters.Values.Select(x => x.Id).Distinct().ToArray();
            OpenLoadingPopup();
            var charactersFullData = await _characterManager.GetCharacterFullInfos(ids);
            Context.CharacterSelection.PickedCharacters = new Dictionary<long, CharacterFullInfo>();
            foreach (var characterFullInfo in charactersFullData)
            {
                var originCharacterId = characters.First(x => x.Value.Id == characterFullInfo.Id).Key;
                Context.CharacterSelection.PickedCharacters[originCharacterId] = characterFullInfo;
            }

            CloseCharacterSelectionOnNextPageLoadingBeginning();
            await MoveNext.Run();
        }
        
        private void OpenLoadingPopup()
        {
            _popupManager.SetupPopup(_loadingPopupConfiguration);
            _popupManager.ShowPopup(_loadingPopupConfiguration.PopupType, true);
        }
        
        protected virtual void CloseCharacterSelection()
        {
            _popupManager.ClosePopupByType(PopupType.CharacterSelection);
            _popupManager.ClosePopupByType(_loadingPopupConfiguration.PopupType); 
        }

        private void CloseCharacterSelectionOnNextPageLoadingBeginning()
        {
            _pageManager.PageSwitchingBegan += OnNextPageLoadingBegun;
            void OnNextPageLoadingBegun(PageId? previousPage, PageData nextPage)
            {
                _pageManager.PageSwitchingBegan -= OnNextPageLoadingBegun;
                CloseCharacterSelection();
            }
        }
        
        private async void OnCancelSelection()
        {
            Context.OnLevelCreationCanceled?.Invoke();
            await MoveBack.Run();
        }
    }
}
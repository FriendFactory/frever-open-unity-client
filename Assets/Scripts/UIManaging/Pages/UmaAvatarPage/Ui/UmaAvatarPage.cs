using System;
using System.Linq;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    internal sealed class UmaAvatarPage : GenericPage<UmaAvatarArgs>
    {
        [SerializeField] private UmaAvatarPanel _avatarPanel;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private Button _backButton;
        [SerializeField] private CharacterSelectionList _characterSelectionList;
        [SerializeField] private Button _createNewCharacterButton;
     
        private int _characterCount;
        private int _charactersLeftToDelete;
        private CharacterCarouselModel _characterCarouselModel;

        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupHelper;
        [Inject] private CharacterManager _characterManager;
        [Inject] private UsersManager _usersManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private PageManager _pageManager;
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        [Inject] private AvatarPageLocalization _localization;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private LocalUserDataHolder _dataHolder;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.AvatarPage;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        private void OnEnable()
        {
            _avatarPanel.ClearCharacters();
            _backButton.onClick.AddListener(OnBackButtonClick);
            _characterSelectionList.CharacterDeleteClicked += DeleteCharacter;
            _characterSelectionList.CharacterEditClicked += EditCharacter;
            _characterSelectionList.CharacterAsMainClicked += OnCharacterSetAsMain;
            _characterSelectionList.CharacterNameChanged += OnNewNameSubmitted;
            _characterSelectionList.CreateNewClicked += CreateNewCharacter;
            _characterSelectionList.gameObject.SetActive(false);
            _createNewCharacterButton.onClick.AddListener(CreateNewCharacter);
        }

        private void OnDisable()
        {
            _avatarPanel.ClearCharacters();
            _backButton.onClick.RemoveListener(OnBackButtonClick);
            _characterSelectionList.CharacterDeleteClicked -= DeleteCharacter;
            _characterSelectionList.CharacterEditClicked -= EditCharacter;
            _characterSelectionList.CharacterAsMainClicked -= OnCharacterSetAsMain;
            _characterSelectionList.CharacterNameChanged -= OnNewNameSubmitted;
            _characterSelectionList.CreateNewClicked -= CreateNewCharacter;
            _createNewCharacterButton.onClick.RemoveListener(CreateNewCharacter);
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager manager)
        {
            _characterManager.CharacterUpdated += OnCharacterUpdated;
        }

        protected override void OnDisplayStart(UmaAvatarArgs args)
        {
            base.OnDisplayStart(args);
            var characters = _characterManager.UserCharacters.Where(c => args.TargetGenders.IsNullOrEmpty() || args.TargetGenders.Contains(c.GenderId)).ToArray();
            _characterCount = characters.Length;
            UpdateCharacterAmountText();
            _avatarPanel.ClearCharacters();
            _avatarPanel.SetupGrid(characters);
            var mainCharacters = _characterManager.RaceMainCharacters.Values.ToArray();
            var selected = characters.FirstOrDefault(x => mainCharacters.Contains(x.Id));
            _avatarPanel.SelectCharacter(selected);
            _avatarPanel.Setup(OnCharacterClicked, CreateNewCharacter);
            InitializeCharactersList(characters);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _characterCarouselModel.Dispose();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCharacterDeleted(CharacterInfo character)
        {
            _charactersLeftToDelete--;
            _characterCount--;

            _avatarPanel.DeleteCharacter(character);
            UpdateCharacterAmountText();

            if (_charactersLeftToDelete <= 0)
            {
                _popupManager.ClosePopupByType(PopupType.Loading);
            }
            _characterCarouselModel.OnCharacterDeleted(character);
            _characterSelectionList.UpdateCarousel();
        }

        private void DeleteCharacter(CharacterInfo character)
        {
            var isMainCharacter = _characterManager.IsMainCharacter(character.Id);

            if (isMainCharacter)
            {
                _popupHelper.ShowAlertPopup(_localization.UnableToDeleteMainCharacterPopupDesc,
                                            _localization.UnableToDeleteMainCharacterPopupTitle);
                return;
            }

            var deleteCharacterPopupConfiguration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3, 
                Title = _localization.DeleteCharacterPopupTitle,
                Description = _localization.DeleteCharacterPopupDesc, 
                YesButtonText = _localization.DeleteCharacterPopupConfirmButton,
                YesButtonSetTextColorRed = true,
                NoButtonText = _localization.DeleteCharacterPopupCancelButton,
                OnYes = () => OnCharacterDeletionConfirmed(character), OnNo = OnCharacterDeletionCanceled
            };

            _popupManager.SetupPopup(deleteCharacterPopupConfiguration);
            _popupManager.ShowPopup(deleteCharacterPopupConfiguration.PopupType, true);
        }

        private async void OnCharacterDeletionConfirmed(CharacterInfo character)
        {
            _popupManager.ClosePopupByType(PopupType.EditCharacter);
            var popupConfiguration = ShowLoadingPopup(_localization.DeletingCharacterLoadingTitle);

            var success = await _characterManager.DeleteCharacter(character);
            if (!success) return;

            OnCharacterDeleted(character);
            _avatarPanel.DeleteCharacter(character);
            _popupManager.ClosePopupByType(popupConfiguration.PopupType);

            ShowInformationSnackBar(_localization.CharacterDeletedSnackbarMsg);
        }

        private void OnCharacterDeletionCanceled()
        {
            _popupManager.ClosePopupByType(PopupType.Dialog);
        }

        private InformationPopupConfiguration ShowLoadingPopup(string popupText)
        {
            var loadingPopupConfig = new InformationPopupConfiguration
            {
                PopupType = PopupType.Loading, Title = popupText
            };
            _popupManager.SetupPopup(loadingPopupConfig);
            _popupManager.ShowPopup(loadingPopupConfig.PopupType);
            return loadingPopupConfig;
        }

        private void OnCharacterClicked(CharacterInfo character)
        {
            var characterIndex = Array.IndexOf(_characterCarouselModel.Characters, character);
            _characterSelectionList.Show(characterIndex);
        }

        private void OnCharacterSetAsMain(CharacterInfo character)
        {
            _avatarPanel.SelectCharacter(character);
            _characterManager.SelectCharacter(character);
            var raceId = _metadataProvider.MetadataStartPack.GetRaceByGenderId(character.GenderId).Id;
            var universe = _metadataProvider.MetadataStartPack.GetUniverseByRaceId(raceId);
            _usersManager.UpdateMainCharacterIdForLocalUserOnServer(character.Id, universe.Id);
            _dataHolder.SetMainCharacter(character);
            ShowSuccessSnackBar(_localization.MainCharacterUpdatedSnackbarMsg);
        }

        private void EditCharacter(CharacterInfo character)
        {
            _popupManager.ClosePopupByType(PopupType.EditCharacter);
            OpenEditorPage(character);
        }

        private async void OpenEditorPage(CharacterInfo character)
        {
            var fullCharacterResp = await _characterManager.GetCharacterAsync(character.Id);
            _scenarioManager.ExecuteCharacterEditing(fullCharacterResp);
        }

        private void OnCharacterUpdated(CharacterInfo character)
        {
            _avatarPanel.UpdateCharacter(character);
        }

        private void UpdateCharacterAmountText()
        {
            _headerText.text = $"{_localization.AvatarPageHeader} {_characterCount}/{_characterManager.MaxCharactersCount}";
        }
        
        private void ShowInformationSnackBar(string message)
        {
            _snackBarHelper.ShowInformationSnackBar(message, 2);
        }

        private void ShowSuccessSnackBar(string message)
        {
            _snackBarHelper.ShowSuccessSnackBar(message, 2);
        }

        private void OnBackButtonClick()
        {
            _pageManager.MoveBack();
        }

        private void CreateNewCharacter()
        {
            long raceId;
            var activeUniverses = _metadataProvider.MetadataStartPack.GetActiveUniverses().ToArray();
            if (activeUniverses.Length == 1)
            {
                var universe = activeUniverses[0];
                raceId = universe.Races.First().RaceId;
            }
            else
            {
                raceId = _metadataProvider.MetadataStartPack.GetRaces()
                                          .First(r => r.Genders.Any(g => OpenPageArgs.TargetGenders.Contains(g.Id))).Id;
            }
            
            if (_characterManager.MaxCountReached(raceId))
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.MaxCharacterCountReachedSnackbarMsg, 2);
                return;
            }
            
            _scenarioManager.ExecuteNewCharacterCreation(raceId);
        }

        private void OnNewNameSubmitted(CharacterInfo character, string newName)
        {
            _characterManager.SetNameForCharacter(character, newName);

        }

        private void InitializeCharactersList(CharacterInfo[] targetCharacters)
        {
            _characterCarouselModel = new CharacterCarouselModel(_characterManager, _characterThumbnailsDownloader, targetCharacters);
            _characterSelectionList.Initialize(_characterCarouselModel);
        }
    }
}
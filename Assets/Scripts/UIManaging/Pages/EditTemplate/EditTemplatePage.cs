using System.Threading.Tasks;
using AdvancedInputFieldPlugin;
using Bridge;
using Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using Navigation.Core;
using Newtonsoft.Json;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.Pages.CreatorScore;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.EditTemplate
{
    public class EditTemplatePage : GenericPage<EditTemplatePageArgs>
    {
        private const int MIN_TEMPLATE_NAME_LENGTH = 4;
        private const int MAX_TEMPLATE_NAME_LENGTH = 25;
        
        [SerializeField] private PageHeaderActionView _pageHeaderView;
        [SerializeField] private AdvancedInputField _inputField;
        [SerializeField] private Button _renameButton;
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private GameObject _generateHeader;

        [SerializeField] private GameObject[] _templateNameObjects;
        
        [SerializeField] private GameObject _defaultLayout;
        [SerializeField] private GameObject _lockedLayout;
        
        [SerializeField] private EditTemplateLocalization _localization;
        
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private CreatorScoreHelper _creatorScoreHelper;

        private bool _privacyMessageDisplayed;

        public override PageId Id => PageId.EditTemplate;
        
        protected override void OnInit(PageManager pageManager)
        {
            _renameButton.onClick.AddListener(RenameChallenge);
        }

        protected override void OnDisplayStart(EditTemplatePageArgs args)
        {
            base.OnDisplayStart(args);
            _pageHeaderView.Init(new PageHeaderActionArgs(null, new ButtonArgs(string.Empty, OnBackButtonClick), null));

            var isUnlocked = args.IsTemplateCreationUnlocked || args.OpenForRename;
            
            _defaultLayout.SetActive(isUnlocked);
            _lockedLayout.SetActive(!isUnlocked);
            _errorText.SetActive(false);
            
            if (!isUnlocked)
            {
                return;
            }
            
            _generateHeader.SetActive(!args.OpenForRename);

            EnableTemplateNameObjects(true);
            
            _inputField.Text = args.TemplateName ?? string.Empty;
            _errorText.text = string.Empty;

            _privacyMessageDisplayed = false;
            _renameButton.interactable = !string.IsNullOrEmpty(args.TemplateName);
            
            _inputField.OnValueChanged.AddListener(OnNameInputChanged);
        }
        
        private void OnNameInputChanged(string input)
        {
            _renameButton.interactable = !string.IsNullOrEmpty(input);
        }

        private void OnBackButtonClick()
        {
            OpenPageArgs.BackButtonCallback?.Invoke();
            _pageManager.MoveBack();
        }

        private async void RenameChallenge()
        {
            if (!OpenPageArgs.IsVideoPublic && !_privacyMessageDisplayed)
            {
                ShowPublicAccessRequiredForTemplatePopup();
                return;
            }
            
            _renameButton.interactable = false;
            
            var newName = _inputField.Text.TrimEnd();
            
            var error = string.Empty;
            if (newName.Length < MIN_TEMPLATE_NAME_LENGTH || string.IsNullOrWhiteSpace(newName))
            {
                error = string.Format(_localization.TemplateNameMinLengthMessage, MIN_TEMPLATE_NAME_LENGTH);
            }
            
            if (newName.Length > MAX_TEMPLATE_NAME_LENGTH)
            {
                error = string.Format(_localization.TemplateNameMaxLengthMessage, MAX_TEMPLATE_NAME_LENGTH);
            }

            _errorText.text = error;
            var isNameValid = string.IsNullOrEmpty(error);
            if (!isNameValid)
            {
                _renameButton.interactable = true;
                _errorText.SetActive(true);
                return;
            }

            var isNameAvailable = await CheckTemplateNameAvailable(newName);
            if (!isNameAvailable)
            {
                _renameButton.interactable = true;
                _errorText.SetActive(true);
                return;
            }

            OpenPageArgs.NameUpdatedCallback?.Invoke(true, newName);

            _pageManager.MoveBack();
        }

        private async Task<bool> CheckTemplateNameAvailable(string input)
        {
            var result = await _bridge.CheckTemplateName(input);

            if (result.IsSuccess) return true;
            
            var error = JsonConvert.DeserializeObject<ServerError>(result.ErrorMessage);
            switch (error.ErrorCode)
            {
                case Constants.ErrorMessage.TEMPLATE_MODERATION_FAILED_ERROR_CODE:
                    _errorText.text = _localization.TemplateNameErrorMessage;
                    break;
                default:
                    _errorText.text = error.ErrorCode;
                    break;
            }
            
            return false;
        }
        
        private void ShowPublicAccessRequiredForTemplatePopup()
        {
            var config = new DialogDarkPopupConfiguration()
            {
                Title = _localization.TemplateVideoPrivacyPopupTitle,
                Description = _localization.TemplateVideoPrivacyPopupText,
                YesButtonText = _localization.TemplateVideoPrivacyPopupYesText,
                NoButtonText = _localization.TemplateVideoPrivacyPopupNoText,
                OnYes = PrivacyUpdateConfirmed,
                PopupType = PopupType.DialogDark
            };
            
            _popupManager.PushPopupToQueue(config);
        }

        private void PrivacyUpdateConfirmed()
        {
            OpenPageArgs.SetVideoPublicCallback?.Invoke();
            _snackBarHelper.ShowInformationSnackBar(_localization.VideoPrivacyUpdatedSnackbarMessage);
            _privacyMessageDisplayed = true;
            RenameChallenge();
        }

        private void EnableTemplateNameObjects(bool isEnabled)
        {
            foreach (var obj in _templateNameObjects)
            {
                obj.SetActive(isEnabled);
            }
        }
    }
}
using System;
using System.Threading;
using AdvancedInputFieldPlugin;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.InputFields;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.OnBoardingPage.UI;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.EditProfilePage.UI
{
    internal sealed class EditBioPage : AnimatedGenericPage<EditBioPageArgs>
    {
        private const string MODERATION_FAILED_ERROR = "This profile bio is inappropriate.";

        [Header("Header")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _actionButton;
        [SerializeField] private LoadingIndicator _loadingIndicator;
        [Header("Page")]
        [SerializeField] private AdvancedInputField _bioInputField;
        [SerializeField] private TMP_Text _charactersCount;
        [SerializeField] private CharacterLimitFilter _characterLimitFilter;
        [SerializeField] private TextMeshProUGUI _errorMessage;
        
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _userDataHolder;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;
        [Inject] private PopupManager _popupManager;

        private IInputFieldAdapter _inputFieldAdapter;
        private string _originalBio;
        private CancellationTokenSource _tokenSource;
        private EditBioPageLoc _loc;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override PageId Id => PageId.EditBio;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _loc = GetComponent<EditBioPageLoc>();

            _backButton.onClick.AddListener(OnBackButtonClicked);
            _actionButton.onClick.AddListener(OnSaveButtonClicked);

            HideErrorMessage();
        }
        
        protected override void OnDisplayStart(EditBioPageArgs args)
        {
            base.OnDisplayStart(args);

            _tokenSource = new CancellationTokenSource();

            _inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_bioInputField);
            _inputFieldAdapter.OnValueChanged -= OnInputChanged;
            _inputFieldAdapter.OnValueChanged += OnInputChanged;

            RefreshBioInputField();
            HideErrorMessage();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void CleanUp()
        {
            _inputFieldAdapter.Dispose();

            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RefreshBioInputField()
        {
            _originalBio = OpenPageArgs.Bio;
            ResetInputToOriginalValue();
        }

        private void OnBackButtonClicked()
        {
            if (_inputFieldAdapter.Text == _originalBio)
            {
                _pageManager.MoveBack();
            }
            else
            {
                ShowConfirmationDialog();
            }
        }

        private async void OnSaveButtonClicked()
        {
            var inputText = _inputFieldAdapter.Text;
            if (inputText == _originalBio)
            {
                OnSaveSuccess();
                return;
            }

            _actionButton.SetActive(false);
            _loadingIndicator.SetActive(true);

            var result = await _userDataHolder.SetProfileBio(inputText);
            if (result.IsSuccess)
            {
                OnSaveSuccess();
            }
            else
            {
                var errorMessage = result.ErrorMessage.Contains(MODERATION_FAILED_ERROR)
                    ? _loc.BioModerationError
                    : result.ErrorMessage;
                OnSaveFailed(errorMessage);
            }
        }

        private void OnSaveSuccess()
        {
            HideErrorMessage();

            _actionButton.SetActive(true);
            _loadingIndicator.SetActive(false);

            _snackBarHelper.ShowSuccessDarkSnackBar(_loc.BioChanged, 2);
            _pageManager.MoveBack();
        }
        
        private void OnSaveFailed(string errorMessage)
        {
            ShowErrorMessage(errorMessage);

            _actionButton.SetActive(true);
            _loadingIndicator.SetActive(false);
        }

        private void ShowErrorMessage(string message)
        {
            _errorMessage.SetActive(true);
            _errorMessage.text = message;
        }

        private void HideErrorMessage()
        {
            _errorMessage.SetActive(false);
        }

        private void OnInputChanged(string inputText)
        {
            UpdateCharactersCounter(inputText);
        }

        private void ResetInputToOriginalValue()
        {
            _inputFieldAdapter.SetTextWithoutNotify(_originalBio);
            UpdateCharactersCounter(_originalBio);
        }

        private void UpdateCharactersCounter(string inputText)
        {
            var characterCount = _characterLimitFilter.CharacterLimit - _characterLimitFilter.GetCharacterCount(inputText);
            _charactersCount.text = $"{characterCount}";
        }

        private void ShowConfirmationDialog()
        {
            var confirmPopupConfiguration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDark,
                Title = _loc.BioConfirmationTitle,
                Description = _loc.BioConfirmationDesc,
                YesButtonSetTextColorRed = true,
                YesButtonText = _loc.BioConfirmationPositive,
                NoButtonText = _loc.BioConfirmationNegative,
                OnYes = _pageManager.MoveBack
            };

            _popupManager.SetupPopup(confirmPopupConfiguration);
            _popupManager.ShowPopup(confirmPopupConfiguration.PopupType);
        }
    }
}
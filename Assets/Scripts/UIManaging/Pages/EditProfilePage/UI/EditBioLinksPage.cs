using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
using static Common.Constants.ProfileLinks;

namespace UIManaging.Pages.EditProfilePage.UI
{
    internal sealed class EditBioLinksPage : AnimatedGenericPage<EditBioLinksPageArgs>
    {
        private const string PROTOCOL_REGEX = @"^http(s?):\/\/.*";
        private const string TIKTOK_REGEX = @"^http(s?):\/\/[0-9a-zA-Z]{0,3}\.?tiktok\.com\/[-a-zA-Z0-9@:%._\+~#=]{1,256}";
        private const string INSTAGRAM_REGEX = @"^http(s?):\/\/[0-9a-zA-Z]{0,3}\.?instagram\.com\/[-a-zA-Z0-9@:%._\+~#=]{1,256}";
        private const string YOUTUBE_REGEX = @"^http(s?):\/\/[0-9a-zA-Z]{0,3}\.?youtube\.com\/[-a-zA-Z0-9@:%._\+~#=]{1,256}";

        [Header("Header")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _actionButton;
        [SerializeField] private LoadingIndicator _loadingIndicator;
        [Header("Page")]
        [SerializeField] private AdvancedInputField _tiktokInputField;
        [SerializeField] private AdvancedInputField _instagramInputField;
        [SerializeField] private AdvancedInputField _youtubeInputField;
        [Space]
        [SerializeField] private TextMeshProUGUI _errorMessage;
        
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;
        [Inject] private PopupManager _popupManager;

        private IInputFieldAdapter _tiktokInputAdapter;
        private IInputFieldAdapter _instagramInputAdapter;
        private IInputFieldAdapter _youtubeInputAdapter;

        private Dictionary<string, string> _originalLinks;

        private string _tiktokOriginalLink;
        private string _instagramOriginalLink;
        private string _youtubeOriginalLink;

        private CancellationTokenSource _tokenSource;

        private EditBioLinksPageLoc _loc;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override PageId Id => PageId.EditBioLinks;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _loc = GetComponent<EditBioLinksPageLoc>();

            _backButton.onClick.AddListener(OnBackButtonClicked);
            _actionButton.onClick.AddListener(OnSaveButtonClicked);

            HideErrorMessage();
        }
        
        protected override void OnDisplayStart(EditBioLinksPageArgs args)
        {
            base.OnDisplayStart(args);

            _tokenSource = new CancellationTokenSource();

            _tiktokInputAdapter = _inputFieldAdapterFactory.CreateInstance(_tiktokInputField);
            _tiktokInputAdapter.OnValueChanged -= OnLinkChanged;
            _tiktokInputAdapter.OnValueChanged += OnLinkChanged;

            _instagramInputAdapter = _inputFieldAdapterFactory.CreateInstance(_instagramInputField);
            _instagramInputAdapter.OnValueChanged -= OnLinkChanged;
            _instagramInputAdapter.OnValueChanged += OnLinkChanged;

            _youtubeInputAdapter = _inputFieldAdapterFactory.CreateInstance(_youtubeInputField);
            _youtubeInputAdapter.OnValueChanged -= OnLinkChanged;
            _youtubeInputAdapter.OnValueChanged += OnLinkChanged;

            RefreshInputFields();
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
            _tiktokInputAdapter.Dispose();
            _instagramInputAdapter.Dispose();
            _youtubeInputAdapter.Dispose();

            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RefreshInputFields()
        {
            _originalLinks = OpenPageArgs.BioLinks ?? new Dictionary<string, string>();

            _tiktokOriginalLink = _originalLinks.TryGetValue(LINK_KEY_TIKTOK, out var tiktokLink) ? tiktokLink : string.Empty;
            _instagramOriginalLink = _originalLinks.TryGetValue(LINK_KEY_INSTAGRAM, out var instaLink) ? instaLink : string.Empty;
            _youtubeOriginalLink = _originalLinks.TryGetValue(LINK_KEY_YOUTUBE, out var youtubeLink) ? youtubeLink : string.Empty;

            _tiktokInputAdapter.SetTextWithoutNotify(_tiktokOriginalLink);
            _instagramInputAdapter.SetTextWithoutNotify(_instagramOriginalLink);
            _youtubeInputAdapter.SetTextWithoutNotify(_youtubeOriginalLink);
        }

        private void OnBackButtonClicked()
        {
            if (_tiktokInputField.Text == _tiktokOriginalLink &&
                _instagramInputField.Text == _instagramOriginalLink &&
                _youtubeInputField.Text == _youtubeOriginalLink)
            {
                _pageManager.MoveBack();
            }
            else
            {
                ShowConfirmationDialog();
            }
        }

        private void OnSaveButtonClicked()
        {
            ShowLoadingIndicator(true);

            var tiktokLink = _tiktokInputAdapter.Text;
            var instagramLink = _instagramInputAdapter.Text;
            var youtubeLink = _youtubeInputAdapter.Text;

            var isTiktokLinkValid = string.IsNullOrEmpty(tiktokLink) || ValidateTiktokLink(ref tiktokLink);
            var isInstagramLinkValid = string.IsNullOrEmpty(instagramLink) || ValidateInstagramLink(ref instagramLink);
            var isYoutubeLinkValid = string.IsNullOrEmpty(youtubeLink) || ValidateYoutubeLink(ref youtubeLink);

            var areLinksValid = isTiktokLinkValid && isInstagramLinkValid && isYoutubeLinkValid;
            if (areLinksValid)
            {
                SaveLinks(tiktokLink, instagramLink, youtubeLink);
            }
            else
            {
                var links = (!isTiktokLinkValid) ? "TikTok" : "";
                links += (!isInstagramLinkValid) ? (!isTiktokLinkValid) ? ", Instagram" : "Instagram" : "";
                links += (!isYoutubeLinkValid) ? (!isTiktokLinkValid || !isInstagramLinkValid) ? ", YouTube" : "YouTube" : "";

                OnSaveFailed($"{_loc.LinksNotValid}: {links}");
            }
        }

        private bool ValidateTiktokLink(ref string link)
        {
            return ValidateLink(ref link, _tiktokOriginalLink, TIKTOK_REGEX);
        }

        private bool ValidateInstagramLink(ref string link)
        {
            return ValidateLink(ref link, _instagramOriginalLink, INSTAGRAM_REGEX);
        }

        private bool ValidateYoutubeLink(ref string link)
        {
            return ValidateLink(ref link, _youtubeOriginalLink, YOUTUBE_REGEX);
        }

        private static bool ValidateLink(ref string link, string originalLink, string linkRegex)
        {
            var isLinkStartsWithHttp = Regex.IsMatch(link, PROTOCOL_REGEX);
            if (!isLinkStartsWithHttp) link = $"https://{link}";

            var isLinkValid = (link == originalLink) || Regex.IsMatch(link, linkRegex);
            return isLinkValid;
        }

        private async void SaveLinks(string tiktokLink, string instagramLink, string youtubeLink)
        {
            var links = new Dictionary<string, string>()
            {
                {LINK_KEY_TIKTOK, tiktokLink},
                {LINK_KEY_INSTAGRAM, instagramLink},
                {LINK_KEY_YOUTUBE, youtubeLink}
            };

            var result = await _localUserDataHolder.SetProfileBioLinks(links);
            if (result.IsSuccess)
            {
                OnSaveSuccess();
            }
            else
            {
                OnSaveFailed(result.ErrorMessage);
            }
        }

        private void OnSaveSuccess()
        {

            HideErrorMessage();
            ShowLoadingIndicator(false);

            _snackBarHelper.ShowSuccessDarkSnackBar(_loc.LinksChanged, 2);
            _pageManager.MoveBack();
        }
        
        private void OnSaveFailed(string errorMessage)
        {
            ShowErrorMessage(errorMessage);
            ShowLoadingIndicator(false);
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

        private void OnLinkChanged(string link)
        {
            HideErrorMessage();
        }

        private void ShowLoadingIndicator(bool show)
        {
            _actionButton.SetActive(!show);
            _loadingIndicator.SetActive(show);
        }

        private void ShowConfirmationDialog()
        {
            var confirmPopupConfiguration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDark,
                Title = _loc.LinksConfirmationTitle,
                Description = _loc.LinksConfirmationDesc,
                YesButtonSetTextColorRed = true,
                YesButtonText = _loc.LinksConfirmationPositive,
                NoButtonText = _loc.LinksConfirmationNegative,
                OnYes = _pageManager.MoveBack
            };

            _popupManager.SetupPopup(confirmPopupConfiguration);
            _popupManager.ShowPopup(confirmPopupConfiguration.PopupType);
        }
    }
}
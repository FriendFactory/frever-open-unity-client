using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Amplitude;
using System.Threading;
using Extensions;
using Modules.CharacterManagement;
using Modules.SignUp;
using Navigation.Core;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.Common.UsersManagement;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UiManaging.Pages.OnBoardingPage.UI.Pages;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal sealed class OnBoardingUsernamePage : OnBoardingTextInputPage
    {
        private const float REQUIREMENTS_UPDATE_DELAY = 0.5f;
        private const int USERNAME_SUGGESTIONS_AMOUNT = 3;
    
        [Space]
        [SerializeField] private List<RequirementField> _requirementFields;
        [SerializeField] private UsernameLabel _usernameLabelPrefab;
        [SerializeField] private GameObject _requirementFieldContainer;
        [SerializeField] private GameObject _usernameTakenContainer;
        [SerializeField] private RectTransform _usernameLabelContainer;
        [SerializeField] private Button _randomizeButton;
        [SerializeField] private RawImage _characterThumbnail;

        [Inject] private ISignUpService _signUpService;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private CharacterManager _characterManager;
        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        
        private readonly List<UsernameLabel> _usernameLabels = new List<UsernameLabel>();
        private Coroutine _updateCoroutine;
        private CancellationTokenSource _tokenSource;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.OnBoardingUsernamePage;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async void OnDisplayStart(OnBoardingTextInputPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _randomizeButton.onClick.AddListener(OnRandomizeUsername);
            _requirementFieldContainer.SetActive(true);
            _usernameTakenContainer.SetActive(false);

            _tokenSource?.CancelAndDispose();
            _tokenSource = new CancellationTokenSource();
            
            _characterThumbnail.texture = Texture2D.grayTexture;
            _characterThumbnail.color = Color.white;
            DownloadCharacterThumbnail();
            
            if (string.IsNullOrEmpty(args.InitialText))
            {
                IdleRequirementFields();
                var randomName = await _signUpService.GetNextUsernameSuggestion();
                UpdateUsernameInputField(randomName);
            }
            else
            {
                UpdateRequirementFields();
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);

            _randomizeButton.onClick.RemoveListener(OnRandomizeUsername);
            _updateCoroutine = null;
            
            _tokenSource?.CancelAndDispose();
            _tokenSource = null;
        }

        protected override void OnValueChanged(string value)
        {
            base.OnValueChanged(value);
            
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
            }

            ContinueButton.interactable = false;
            
            var isEmpty = string.IsNullOrEmpty(value);
            
            if (isEmpty)
            {
                IdleRequirementFields();
                return;
            }
            
            _updateCoroutine = StartCoroutine(UpdateRequirementFieldsCoroutine());
        }

        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------

        private async void OnRandomizeUsername()
        {
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.RANDOMIZE_USERNAME);
            
            _randomizeButton.interactable = false;
            
            UpdateUsernameInputField(await _signUpService.GetNextUsernameSuggestion());
        }

        private void UpdateUsernameInputField(string value)
        {
            CurrentRegistrationInputField.SetText(value);
            
            _randomizeButton.interactable = true;
            _requirementFieldContainer.SetActive(true);
            _usernameTakenContainer.SetActive(false);
            
            OpenPageArgs.OnInputChanged(value);
            
            if (string.IsNullOrEmpty(value))
            {
                IdleRequirementFields();
            }
            else
            {
                UpdateRequirementFields();
            }
            
            CurrentRegistrationInputField.Select();
        }
        
        private IEnumerator UpdateRequirementFieldsCoroutine()
        {
            yield return new WaitForSeconds(REQUIREMENTS_UPDATE_DELAY);
            
            UpdateRequirementFields();
        }

        private async void UpdateRequirementFields()
        {
            ContinueButton.interactable = false;
            
            foreach (var field in _requirementFields)
            {
                field.UpdateStatus(UsernameRequirementStatus.Loading);
            }

            
            var result = await _signUpService.ValidateUsername();
            var usernameTaken = result.RequirementFailed.ContainsKey(RequirementType.UsernameTaken) 
                             && result.RequirementFailed[RequirementType.UsernameTaken];

            ContinueButton.interactable = result.IsValid;
            
            _requirementFieldContainer.SetActive(!usernameTaken);
            _usernameTakenContainer.SetActive(usernameTaken);

            if (usernameTaken)
            {
                UpdateRandomUsernameSuggestions(await _signUpService.GetUsernameSuggestionList(USERNAME_SUGGESTIONS_AMOUNT));
            }
            else
            {
                foreach (var field in _requirementFields)
                {
                    field.UpdateStatus(result.RequirementFailed.ContainsKey(field.Type) 
                                    && result.RequirementFailed[field.Type] 
                                           ? UsernameRequirementStatus.Incorrect 
                                           : UsernameRequirementStatus.Correct);
                }
            }

            _updateCoroutine = null;
        }

        private void UpdateRandomUsernameSuggestions(List<string> usernameSuggestions)
        {
            var currentUsername = CurrentRegistrationInputField.ApplyTextAlterations();

            // prevents currently selected nickname from appearing on the list
            usernameSuggestions.RemoveAll(suggestion => suggestion.Equals(currentUsername)); 
            
            while (_usernameLabels.Count > usernameSuggestions.Count)
            {
                Destroy(_usernameLabels.Last().gameObject);
                _usernameLabels.RemoveAt(_usernameLabels.Count - 1);
            }
            
            while (_usernameLabels.Count < usernameSuggestions.Count)
            {
                _usernameLabels.Add(Instantiate(_usernameLabelPrefab, _usernameLabelContainer));
            }

            for (var i = 0; i < usernameSuggestions.Count; i++)
            {
                var usernameSuggestion = usernameSuggestions[i];
                _usernameLabels[i].SetUsername(usernameSuggestion, () => UpdateUsernameInputField(usernameSuggestion));
            }
        }

        private void IdleRequirementFields()
        {
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
            }

            ContinueButton.interactable = false;
            
            foreach (var field in _requirementFields)
            {
                field.UpdateStatus(UsernameRequirementStatus.Idle);
            }
        }

        private void DownloadCharacterThumbnail()
        {
            _characterThumbnailsDownloader.GetCharacterThumbnail(_characterManager.SelectedCharacter, Resolution._128x128, 
                                                                 OnTextureDownloaded, OnTextureFailedToDownload, _tokenSource.Token);
        }
        
        private void OnTextureDownloaded(Texture texture2D)
        {
            _characterThumbnail.texture = texture2D;
        }

        private void OnTextureFailedToDownload(string reason)
        {
            _characterThumbnail.texture = Texture2D.grayTexture;
            Debug.LogError(reason);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using AdvancedInputFieldPlugin;
using Common.Abstract;
using Common.Collections;
using Extensions;
using Modules.SignUp;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Common.InputFields.Requirements;
using UIManaging.Pages.EditUsername;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.AccountDetails.UsernameInput
{
    public sealed class UsernameInputPanel: BaseContextPanel<UsernameInputModel>
    {
        private const float INPUT_UPDATE_INTERVAL = 0.5f;

        [SerializeField] private LocalUserPortraitView _localUserPortraitView;
        [SerializeField] private AdvancedInputField _inputField;
        [SerializeField] private Button _randomizeButton;
        [SerializeField] private UsernameRequirementsPanel _usernameRequirementsPanel;
        [SerializeField] private UsernameSuggestionsPanel _usernameSuggestionsPanel;
        
        private Coroutine _updateCoroutine;
        private float _nextUpdateTime;

        public string Input => _inputField.Text;

        public event Action RandomizeRequested;
        
        protected override void OnInitialized()
        {
            _inputField.Text = ContextData.Input;
            
            ContextData.ValidationEvent += OnValidationEvent;
            ContextData.InputValidated += OnInputValidated;
            ContextData.InputRandomized += OnInputRandomized;

            _usernameSuggestionsPanel.SuggestionSelected += OnSuggestionSelected;
            
            _usernameRequirementsPanel.Initialize();
            _usernameSuggestionsPanel.Initialize();
            _localUserPortraitView.Initialize();
            
            _inputField.OnValueChanged.AddListener(OnValueChanged);
            
            _randomizeButton.onClick.AddListener(OnRandomizeRequested);
            
            _usernameSuggestionsPanel.SetActive(false);
        }


        protected override void BeforeCleanUp()
        {
            ContextData.ValidationEvent -= OnValidationEvent;
            ContextData.InputValidated -= OnInputValidated;
            ContextData.InputRandomized -= OnInputRandomized;

            _usernameSuggestionsPanel.SuggestionSelected -= OnSuggestionSelected;
            
            _usernameRequirementsPanel.CleanUp();
            _usernameSuggestionsPanel.CleanUp();
            _localUserPortraitView.CleanUp();
            
            _inputField.OnValueChanged.RemoveListener(OnValueChanged);
            
            _randomizeButton.onClick.RemoveListener(OnRandomizeRequested);
        }

        private void OnValueChanged(string value)
        {
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
            
            _updateCoroutine = StartCoroutine(DelayedModelUpdateCoroutine());
        }

        private IEnumerator DelayedModelUpdateCoroutine()
        {
            yield return new WaitForSeconds(INPUT_UPDATE_INTERVAL);
            
            ContextData.Input = _inputField.Text;
        }

        private void OnValidationEvent(ValidationEventState state)
        {
            switch (state)
            {
                case ValidationEventState.Started:
                    _randomizeButton.interactable = false;
                    ChangeRequirementsState(RequirementValidationState.InProgress);
                    break;
                case ValidationEventState.Finished:
                    ChangeRequirementsState(RequirementValidationState.Valid);
                    _randomizeButton.interactable = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnInputValidated(UsernameValidationResult result)
        {
            _usernameRequirementsPanel.SetActive(!result.UsernameTaken);
            _usernameSuggestionsPanel.SetActive(result.UsernameTaken);

            _updateCoroutine = null;
            
            if (result.IsValid)
            {
                ChangeRequirementsState(RequirementValidationState.Valid);
                return;
            }

            if (result.UsernameTaken)
            {
                UpdateRandomUsernameSuggestions(result.UsernameSuggestions);
                return;
            }
            
            _usernameRequirementsPanel.UpdateRequirements(result.FailedRequirements);
        }

        private void UpdateRandomUsernameSuggestions(List<string> usernameSuggestions)
        {
            // prevents currently selected nickname from appearing on the list
            usernameSuggestions.RemoveAll(suggestion => suggestion.Equals(ContextData.Input));
            
            _usernameSuggestionsPanel.UpdateRandomUsernameSuggestions(usernameSuggestions);
        }

        private void OnSuggestionSelected(string username)
        {
            _inputField.SetText(username);
            ContextData.SetInput(username);
            // assuming that suggestion is always valid
            ContextData.OnInputValidated(new UsernameValidationResult());
        }

        private void OnRandomizeRequested()
        {
            _usernameRequirementsPanel.SetActive(true);
            _usernameSuggestionsPanel.SetActive(false);
            
            RandomizeRequested?.Invoke();
        } 

        private void OnInputRandomized(string username) => OnSuggestionSelected(username);
        private void ChangeRequirementsState(RequirementValidationState state) =>
            _usernameRequirementsPanel.ChangeRequirementsState(state);
    }

    [Serializable]
    internal sealed class RequirementTypesDictionary : SerializedDictionary<RequirementType, InputRequirementInfoPanel>
    {
    }
}
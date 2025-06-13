using System;
using System.Threading.Tasks;
using Modules.SignUp;
using UIManaging.Common.Abstract;
using UIManaging.Pages.Common.AccountDetails.UsernameInput;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace UIManaging.Pages.EditUsername
{
    public sealed class UsernameInputPresenter: GenericPresenter<UsernameInputModel, UsernameInputPanel>
    {
        private const int USERNAME_SUGGESTIONS_AMOUNT = 3;
        
        private readonly ISignUpService _signUpService;
        private readonly LocalUserDataHolder _localUserDataHolder;

        public UsernameInputPresenter(ISignUpService signUpService, LocalUserDataHolder localUserDataHolder)
        {
            _signUpService = signUpService;
            _localUserDataHolder = localUserDataHolder;
        }

        protected override void OnInitialized()
        {
            Model.InputChanged += OnInputChanged;

            View.RandomizeRequested += Randomize;
        }

        protected override void BeforeCleanUp()
        {
            Model.InputChanged -= OnInputChanged;
            
            View.RandomizeRequested -= Randomize;
        }

        private async void Randomize()
        {
            try
            {
                Model.OnValidationEvent(ValidationEventState.Started);
                
                var username = await _signUpService.GetNextUsernameSuggestion();
                Model.IsValid = true;
                
                Model.OnInputRandomized(username);
                Model.OnValidationEvent(ValidationEventState.Finished);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void OnInputChanged(string value)
        {
            var isEmpty = string.IsNullOrEmpty(value);
            
            if (isEmpty) return;

            _signUpService.SetUserName(value);
            
            try
            {
                await ValidateRequirementsAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task ValidateRequirementsAsync()
        {
            Model.OnValidationEvent(ValidationEventState.Started);

            var nameChanged = _signUpService.SelectedUserName != _localUserDataHolder.NickName;
            UsernameValidationResult result;
            if (nameChanged)
            {
                var validationResult = await _signUpService.ValidateUsername();
                var usernameTaken = validationResult.RequirementFailed.ContainsKey(RequirementType.UsernameTaken) 
                                 && validationResult.RequirementFailed[RequirementType.UsernameTaken];
            
                Model.IsValid = validationResult.IsValid;
                
                if (usernameTaken)
                {
                    var suggestions = await _signUpService.GetUsernameSuggestionList(USERNAME_SUGGESTIONS_AMOUNT);
                    result = new UsernameValidationResult(suggestions);
                }
                else
                {
                    result = validationResult.IsValid
                        ? new UsernameValidationResult()
                        : new UsernameValidationResult(validationResult.RequirementFailed);
                }
            }
            else
            {
                result = new UsernameValidationResult();
            }
            
            Model.OnValidationEvent(ValidationEventState.Finished);
            Model.OnInputValidated(result);
        }
    }
}
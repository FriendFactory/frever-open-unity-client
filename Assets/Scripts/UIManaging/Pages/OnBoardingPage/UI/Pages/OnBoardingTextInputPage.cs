using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Modules.SignUp;
using Navigation.Core;
using Newtonsoft.Json;
using TMPro;
using UIManaging.Common.InputFields;
using UIManaging.Pages.Common.RegistrationInputFields;
using UIManaging.Localization;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UiManaging.Pages.OnBoardingPage.UI.Pages
{
    internal class OnBoardingTextInputPage : OnBoardingBasePage<OnBoardingTextInputPageArgs>
    {
        public override PageId Id => PageId.OnBoardingTextInputPage;

        [SerializeField] protected TextMeshProUGUI ValidationText;
        [SerializeField] private SpecializedInputFieldBase[] _registrationInputFields;
        [SerializeField] private Button _changeFlowButton;
        [SerializeField] private Button _validationFailedButton;
        [SerializeField] private TextMeshProUGUI _changeFlowButtonText;
        
        [Inject] protected OnBoardingLocalization Localization;
        [Inject] protected OnboardingServerErrorLocalization ErrorLocalization;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected SpecializedInputFieldBase CurrentRegistrationInputField { get; private set; }
        protected virtual bool ClearInputFieldOnError => false;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void DisplayErrorMessage(string message)
        {
            _validationFailedButton.interactable = true;
            ValidationText.text = OpenPageArgs.FormatValidationText(message);

            if (!ClearInputFieldOnError) return;
            
            CurrentRegistrationInputField.ClearText();
            CurrentRegistrationInputField.Select();
        }

        public void SetContinueButtonInteractivity(bool interactable)
        {
            ContinueButton.interactable = interactable;
        }

        public virtual void ToggleLoadingAnimation(bool active)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnDisplayStart(OnBoardingTextInputPageArgs args)
        {
            if (CurrentRegistrationInputField != null)
            {
                CurrentRegistrationInputField.Hide();
            }
            
            CurrentRegistrationInputField = _registrationInputFields.First(x => x.Type == args.RegistrationType);
            
            var inputFieldModel = new SpecializedInputFieldModel
            {
                InitialText = args.InitialTextFunction?.Invoke() ?? args.InitialText,
                PlaceHolderText = args.PlaceholderText,
                InputType = args.InputType,
                ContentType = args.ContentType,
                CharacterLimit = args.MaxCharactersAmount,
                OnValueChanged = OnValueChanged,
                OnKeyboardStatusChanged = OnKeyboardStatusChanged,
                OnKeyboardSubmit = OnContinueButtonClicked
            };
            CurrentRegistrationInputField.Initialize(inputFieldModel);
            CurrentRegistrationInputField.Display();

            base.OnDisplayStart(args);
            ValidationText.text = "";
            _validationFailedButton.interactable = false;
            SetupChangeFlowButton();

            if (OpenPageArgs.ValidationFailedButtonClicked != null)
            {
                _validationFailedButton.onClick.RemoveAllListeners();
                _validationFailedButton.onClick.AddListener(OpenPageArgs.ValidationFailedButtonClicked);
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            CurrentRegistrationInputField.Hide();
        }

        protected virtual void OnKeyboardStatusChanged(KeyboardStatus status) { }

        protected override async void OnContinueButtonClicked()
        {
            SetContinueButtonInteractivity(false);
            OpenPageArgs.SetFinishedText?.Invoke(CurrentRegistrationInputField.ApplyTextAlterations());
            
            var checkResult = await CanContinue();
            if (!checkResult) return;

            SetContinueButtonInteractivity(true);
            base.OnContinueButtonClicked();
        }

        protected async Task<bool> CanContinue()
        {
            if (OpenPageArgs.ValidationCheck != null)
            {
                var validationCheck = await OpenPageArgs.ValidationCheck.Invoke();
                
                if (!validationCheck.IsValid)
                {
                    var (errorMessage, errorCode, interactable) = OpenPageArgs.GetErrorMessage(validationCheck);
                    DisplayValidationError(errorMessage, errorCode, interactable);
                    return false;
                }
            }

            var allowedToContinue = await IsAllowedToContinue();

            if (!allowedToContinue.IsValid)
            {
                DisplayValidationError(allowedToContinue.ReasonPhrase, allowedToContinue.ErrorCode);
            }
            
            return allowedToContinue.IsValid;
        }

        protected virtual void OnValueChanged(string value)
        {
        #if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            if (OpenPageArgs.InputValidationFunction != null)
            {
                value = OpenPageArgs.InputValidationFunction(value);
                CurrentRegistrationInputField.SetText(value);
            }
        #endif

            OpenPageArgs.OnInputChanged(value);
            RefreshContinueButtonInteractivity();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task<ValidationResult> IsAllowedToContinue()
        {
            var defaultResult = new ValidationResult
            {
                IsValid = true,
            };
            
            var result = OpenPageArgs.IsAllowedToContinue == null ? defaultResult : await OpenPageArgs.IsAllowedToContinue.Invoke();
            
            return result;
        }

        private void SetupChangeFlowButton()
        {
            _changeFlowButton.onClick.RemoveAllListeners();
            _changeFlowButton.onClick.AddListener(OpenPageArgs.ChangeFlowButtonClicked);
            _changeFlowButton.gameObject.SetActive(OpenPageArgs.ShowChangeFlowButton);
            _changeFlowButtonText.text = OpenPageArgs.ChangeFlowButtonText;
        }

        private void DisplayValidationError(string message, string errorCode, bool interactable = true)
        {
            if (errorCode == OnboardingServerErrorLocalization.PhoneNumberRetryNotAvailable)
            {
                // reuse l10n term for this code
                errorCode = OnboardingServerErrorLocalization.PhoneNumberTooManyRequests;
            }

            var localizedError = ErrorLocalization.GetLocalized(errorCode);
            interactable = errorCode != OnboardingServerErrorLocalization.PhoneNumberTooManyRequests;
            
            ValidationText.text = OpenPageArgs.FormatValidationText(localizedError);
            _validationFailedButton.interactable = interactable;
        }
    }
}
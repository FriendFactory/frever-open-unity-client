using System;
using System.Threading.Tasks;
using Modules.SignUp;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.RegistrationInputFields;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UiManaging.Pages.OnBoardingPage.UI.Args
{
    public class OnBoardingTextInputPageArgs : OnBoardingBasePageArgs
    {
        public override PageId TargetPage => PageId.OnBoardingTextInputPage;
        public TMP_InputField.InputType InputType = TMP_InputField.InputType.Standard;
        public string PlaceholderText = string.Empty;
        public TMP_InputField.ContentType ContentType;
        public string InitialText;
        public Func<string> InitialTextFunction;
        public Func<string, string> InputValidationFunction;
        public int MaxCharactersAmount = 25;
        public Func<Task<ValidationResult>> IsAllowedToContinue;
        public Action<string> DisplayErrorMessage;
        public Action<string> SetFinishedText;
        public Action<string> OnInputChanged;
        public SpecializationType RegistrationType;
        public bool ShowChangeFlowButton = false;
        public Func<Task<ValidationResult>> ValidationCheck;
        public string ChangeFlowButtonText;
        public string ContinueButtonText;
        public string ValidationErrorCodeOverride;
        public UnityAction ChangeFlowButtonClicked;
        public UnityAction ValidationFailedButtonClicked;
        public bool UsingPhoneNumber;
        public Func<string, string> FormatValidationText = s => s;

        public virtual (string, string, bool) GetErrorMessage(ValidationResult validationCheck) =>
            (validationCheck.ReasonPhrase, ValidationErrorCodeOverride ?? validationCheck.ErrorCode, true);
    }
}
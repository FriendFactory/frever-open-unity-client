using System;
using Common.Abstract;
using TMPro;
using UIManaging.Pages.AccountVerification.VerificationMethodInput;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.AccountVerification
{
    internal sealed class VerificationMethodUpdateView: BaseContextView<VerificationMethodUpdateViewModel>, IVerificationMethodView
    {
        [SerializeField] private TextMeshProUGUI _header;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private VerificationMethodInputsHandler _verificationMethodInputsHandler;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TMP_Text _continueButtonLabel;
        [SerializeField] private Button _backButton;
        [SerializeField] private GameObject _loadingIndicator;

        public event Action NextRequested;
        public event Action BackRequested;

        public void ToggleLoading(bool isOn)
        {
            _loadingIndicator.SetActive(isOn);
        }

        public void ShowValidationError(string error)
        {
            _verificationMethodInputsHandler.ShowValidationError(error);
        }

        protected override void OnInitialized()
        {
            _continueButton.interactable = false;
            
            _header.text = ContextData.TextData.Header;
            _description.text = ContextData.TextData.Description;
            _continueButtonLabel.text = ContextData.TextData.ContinueButtonLabel;
            
            var method = ContextData.VerificationMethod;
            
            _verificationMethodInputsHandler.Initialize(method);

            _verificationMethodInputsHandler.InputValidated += OnInputValidated;
            
            _continueButton.onClick.AddListener(OnNextButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        protected override void OnShow()
        {
            _verificationMethodInputsHandler.Select();
        }

        protected override void BeforeCleanUp()
        {
            _verificationMethodInputsHandler.InputValidated -= OnInputValidated;
            
            _verificationMethodInputsHandler.CleanUp();
            
            _continueButton.onClick.RemoveListener(OnNextButtonClicked);
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        private void OnNextButtonClicked() => NextRequested?.Invoke();

        private void OnBackButtonClicked() => BackRequested?.Invoke();

        private void OnInputValidated(bool isValid)
        {
            _continueButton.interactable = isValid;
        }
    }
}
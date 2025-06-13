using System;
using System.Collections.Generic;
using Bridge.AccountVerification.Models;
using Common.Abstract;
using Modules.AccountVerification;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.AccountVerification.VerificationMethodInput;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class VerifyUserView: BaseContextView<VerificationMethodUpdateFlowModel>, IVerificationMethodView
    {
        [SerializeField] private VerificationMethodInputsHandler _verificationMethodInputsHandler;
        [SerializeField] private UserVerificationMethodSwitcher _verificationMethodSwitcher;
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private LoadingButton _nextButton;
        [Header("Animations")] 
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedBehaviour;
        
        [Inject] private AccountVerificationLocalization _localization;
        
        public event Action NextRequested;
        public event Action BackRequested;

        public void ToggleLoading(bool isOn)
        {
            _nextButton.ToggleLoading(isOn);
        }

        public void ShowValidationError(string error)
        {
            _verificationMethodInputsHandler.ShowValidationError(error);
        }

        protected override void OnInitialized()
        {
            UpdateNextButtonLabel();

            _verificationMethodInputsHandler.Initialize(ContextData.UserVerificationMethod);
            _verificationMethodInputsHandler.InputValidated += OnInputValidated;
            
            _verificationMethodSwitcher.Initialize(ContextData.UserVerificationMethod.Type);
            _verificationMethodSwitcher.VerificationMethodTypeChanged += OnVerificationMethodTypeChanged;
            
            _closeButtons.ForEach(button => button.onClick.AddListener(OnCloseRequested));

            _nextButton.Interactable = false;
            _nextButton.Clicked += OnNextRequested;
        }

        protected override void BeforeCleanUp()
        {
            _verificationMethodInputsHandler.InputValidated -= OnInputValidated;
            _verificationMethodInputsHandler.CleanUp();
            
            _verificationMethodSwitcher.VerificationMethodTypeChanged -= OnVerificationMethodTypeChanged;
            _verificationMethodSwitcher.CleanUp();
            
            _closeButtons.ForEach(button => button.onClick.RemoveListener(OnCloseRequested));
            _nextButton.Clicked -= OnNextRequested;
        }

        protected override void OnShow()
        {
            _animatedBehaviour.PlayInAnimation(() => _verificationMethodInputsHandler.Select());
        }

        protected override void OnHide()
        {
            _animatedBehaviour.PlayOutAnimation(() => ToggleLoading(false));
        }

        private void OnNextRequested() => NextRequested?.Invoke();
        private void OnCloseRequested() => BackRequested?.Invoke();
        private void OnInputValidated(bool isValid) => _nextButton.Interactable = isValid;

        private void UpdateNextButtonLabel()
        {
            var textData = _localization.GetVerifyUserPageData(ContextData.UserVerificationMethod);
            
            _nextButton.Label = textData.ContinueButtonLabel;
        }

        private void OnVerificationMethodTypeChanged(CredentialType type)
        {
            ContextData.UserVerificationMethod = new VerificationMethod(type);
            
            if (type.IsLinkable()) return;
            
            _verificationMethodInputsHandler.ChangeVerificationMethod(ContextData.UserVerificationMethod);

            _nextButton.Interactable = false;
            UpdateNextButtonLabel();
        }
    }
}
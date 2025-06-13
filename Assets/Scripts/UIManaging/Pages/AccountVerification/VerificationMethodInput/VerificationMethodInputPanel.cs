using System;
using AdvancedInputFieldPlugin;
using Common.Abstract;
using Extensions;
using Modules.AccountVerification;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal class VerificationMethodInputPanel: BaseContextPanel<VerificationMethodInputViewModel>
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] protected AdvancedInputField _inputField;
        [SerializeField] private TMP_Text _validationError;
        
        public event Action<bool> InputValidated;
        
        public IVerificationMethod VerificationMethod => ContextData.Method;

        public virtual void Select() { }
        
        protected override void OnInitialized()
        {
            _header.SetActive(ContextData.ShowHeader);
            
            _inputField.OnValueChanged.AddListener(OnValueChanged);

            VerificationMethod.InputValidated += OnInputValidated;
        }

        protected override void BeforeCleanUp()
        {
            _inputField.OnValueChanged.RemoveListener(OnValueChanged);
            
            _inputField.Text = string.Empty;
            
            _validationError.SetActive(false);
            
            VerificationMethod.InputValidated -= OnInputValidated;
        }
        
        protected virtual void OnValueChanged(string value)
        {
            VerificationMethod.Input = value;

            _validationError.text = string.Empty;
            _validationError.SetActive(false);
        }

        protected virtual void OnInputValidated(bool isValid)
        {
            InputValidated?.Invoke(isValid);
        }

        public void ShowValidationError(string message)
        {
            _validationError.SetActive(true);
            _validationError.text = message;
        }
    }
}
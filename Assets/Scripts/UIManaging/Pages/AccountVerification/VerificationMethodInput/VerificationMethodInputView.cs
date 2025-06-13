using AdvancedInputFieldPlugin;
using Common.Abstract;
using Modules.AccountVerification;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal sealed class VerificationMethodInputView: BaseContextPanel<IVerificationMethod>
    {
        [SerializeField] private AdvancedInputField _inputField;
        [SerializeField] private TMP_Text _validationError;
        
        protected override void OnInitialized()
        {
            _inputField.OnValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(string value)
        {
            
        }
    }
}
using UIManaging.Common.InputFields.Requirements;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal sealed class PasswordUpdateVerificationMethodInputPanel: PasswordVerificationMethodInputPanel
    {
        [SerializeField] private PasswordRequirementInfoPanel _passwordRequirementInfoPanel;

        private PasswordRequirement PasswordRequirement { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            PasswordRequirement = new PasswordRequirement();
            
            _passwordRequirementInfoPanel.Initialize(PasswordRequirement);
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _passwordRequirementInfoPanel.CleanUp();
        }

        protected override void OnInputValidated(bool isValid)
        {
            PasswordRequirement.ValidationState = isValid ? RequirementValidationState.Valid : RequirementValidationState.Invalid;
            base.OnInputValidated(isValid);
        }
    }
}
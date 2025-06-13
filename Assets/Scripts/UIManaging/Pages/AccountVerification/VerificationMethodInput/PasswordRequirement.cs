using System;
using UIManaging.Common.InputFields.Requirements;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal class PasswordRequirement
    {
        public RequirementValidationState ValidationState
        {
            get => _validationState;
            set
            {
                if (_validationState == value) return;

                _validationState = value;
                
                ValidationStateChanged?.Invoke(_validationState);
            }
        }

        public event Action<RequirementValidationState> ValidationStateChanged;

        private RequirementValidationState _validationState;

        public PasswordRequirement(RequirementValidationState validationState = RequirementValidationState.Idle)
        {
            _validationState = validationState;
        }
    }
}
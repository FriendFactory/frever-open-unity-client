using System;
using Modules.AccountVerification;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    public sealed class VerificationMethodUpdateFlowModel
    {
        public IVerificationMethod TargetVerificationMethod { get; }
        public IVerificationMethod UserVerificationMethod
        {
            get => _userVerificationMethod;
            set
            {
                if (_userVerificationMethod.Type == value.Type) return;

                _userVerificationMethod = value;
                
                UserVerificationMethodChanged?.Invoke(_userVerificationMethod);
            }
        }
        
        public VerificationMethodOperationType NextOperationType { get; }

        public event Action<IVerificationMethod> UserVerificationMethodChanged;

        private IVerificationMethod _userVerificationMethod;
        
        public VerificationMethodUpdateFlowModel(IVerificationMethod targetVerificationMethod, VerificationMethodOperationType nextOperationType)
        {
            _userVerificationMethod = targetVerificationMethod;
            TargetVerificationMethod = targetVerificationMethod;
            NextOperationType = nextOperationType;
        }
    }
}
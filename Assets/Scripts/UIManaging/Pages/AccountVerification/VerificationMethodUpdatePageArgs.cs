using Modules.AccountVerification;
using Navigation.Core;

namespace UIManaging.Pages.AccountVerification
{
    public sealed class VerificationMethodUpdatePageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.VerificationMethodUpdate;
        
        public IVerificationMethod VerificationMethod { get; }
        public VerificationMethodOperationType OperationType { get; }

        internal VerificationMethodUpdatePageArgs(IVerificationMethod verificationMethod, VerificationMethodOperationType operationType)
        {
            VerificationMethod = verificationMethod;
            OperationType = operationType;
        }
    }
}
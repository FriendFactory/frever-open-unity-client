using System.Threading.Tasks;
using Modules.AccountVerification;
using Modules.AccountVerification.Events;
using Navigation.Core;
using StansAssets.Foundation.Patterns;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    public abstract class VerificationFlowHandler
    {
        protected readonly AccountVerificationService AccountVerificationService;
        protected readonly PageManager PageManager;
        
        protected IVerificationMethod UserVerificationMethod => VerificationMethodUpdateFlowModel.UserVerificationMethod;
        protected IVerificationMethod TargetVerificationMethod => VerificationMethodUpdateFlowModel.TargetVerificationMethod;
        protected VerificationMethodOperationType OperationType => VerificationMethodUpdateFlowModel.NextOperationType;
        
        private VerificationMethodUpdateFlowModel VerificationMethodUpdateFlowModel { get; }

        protected VerificationFlowHandler(VerificationMethodUpdateFlowModel verificationMethodUpdateFlowModel, AccountVerificationService accountVerificationService, PageManager pageManager)
        {
            VerificationMethodUpdateFlowModel = verificationMethodUpdateFlowModel;
            AccountVerificationService = accountVerificationService;
            PageManager = pageManager;
        }

        protected void Complete(VerificationMethodUpdateResult result)
        {
            StaticBus<VerificationMethodUpdatedEvent>.Post(new VerificationMethodUpdatedEvent(result));
        }

        public abstract Task<VerificationResult> VerifyCredentialsAsync();
        public abstract void MoveNext();
    }
}
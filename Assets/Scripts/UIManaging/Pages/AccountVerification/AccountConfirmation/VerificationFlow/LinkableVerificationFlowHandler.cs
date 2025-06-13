using System;
using System.Threading.Tasks;
using Modules.AccountVerification;
using Modules.AccountVerification.Events;
using Modules.AccountVerification.Providers;
using Navigation.Core;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class LinkableVerificationFlowHandler : VerificationFlowHandler
    {
        private readonly ICredentialsHandler _credentialsHandler;

        public LinkableVerificationFlowHandler(VerificationMethodUpdateFlowModel verificationMethodUpdateFlowModel,
            AccountVerificationService accountVerificationService, PageManager pageManager, ICredentialsHandler credentialsHandler) : base(verificationMethodUpdateFlowModel, accountVerificationService, pageManager)
        {
            _credentialsHandler = credentialsHandler;
        }

        public override async Task<VerificationResult> VerifyCredentialsAsync()
        {
            var requestResult = await _credentialsHandler.RequestCredentialsAsync();

            if (requestResult.IsCanceled)
            {
                return new VerificationResult(true);
            }
            
            if (requestResult.IsError)
            {
                return new VerificationResult(string.Empty, requestResult.ErrorMessage);
            }

            UserVerificationMethod.Input = requestResult.AppleId;
            UserVerificationMethod.VerificationToken = requestResult.AppleToken;
            
            var verifyCredentialsResult = await AccountVerificationService.VerifyCredentialsAsync(UserVerificationMethod);
            if (verifyCredentialsResult.IsError)
            {
                return new VerificationResult(verifyCredentialsResult.ErrorCode, verifyCredentialsResult.ErrorMessage);
            }

            var verifyUserResponse = verifyCredentialsResult.Model;
            if (!verifyUserResponse.IsSuccessful)
            {
                return new VerificationResult(verifyUserResponse.ErrorCode, verifyUserResponse.ErrorMessage);
            }

            TargetVerificationMethod.VerificationToken = verifyCredentialsResult.Model.VerificationToken;

            return new VerificationResult();
        }

        public override async void MoveNext()
        {
            try
            {
                switch (OperationType)
                {
                    case VerificationMethodOperationType.Change:
                        PageManager.MoveNext( new VerificationMethodUpdatePageArgs(TargetVerificationMethod, VerificationMethodOperationType.Change));
                        break;
                    case VerificationMethodOperationType.Remove:
                        await RemoveVerificationMethodAsync();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task RemoveVerificationMethodAsync()
        {
            try
            {
                var methodUpdateResult = new VerificationMethodUpdateResult(TargetVerificationMethod.Type, VerificationMethodOperationType.Remove);
                
                var result = await AccountVerificationService.RemoveVerificationMethodAsync(TargetVerificationMethod);
                if (result.IsError)
                {
                    methodUpdateResult.IsSuccess = false;
                    return;
                }
                
                Complete(methodUpdateResult);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
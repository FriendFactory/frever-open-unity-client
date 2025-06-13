using System;
using System.Threading.Tasks;
using Modules.AccountVerification;
using Modules.AccountVerification.Events;
using Navigation.Core;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class PasswordVerificationFlowHandler: VerificationFlowHandler
    {
        public PasswordVerificationFlowHandler(VerificationMethodUpdateFlowModel verificationMethodUpdateFlowModel, AccountVerificationService accountVerificationService, PageManager pageManager) : base(verificationMethodUpdateFlowModel, accountVerificationService, pageManager)
        {
        }

        public override async Task<VerificationResult> VerifyCredentialsAsync()
        {
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
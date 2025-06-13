using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using Modules.AccountVerification;
using Modules.AccountVerification.Events;
using Modules.AccountVerification.LoginMethods;
using Navigation.Core;
using UIManaging.Pages.OnBoardingPage.UI.Args;
using UIManaging.SnackBarSystem;
using UnityEngine;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    internal sealed class EmailPhoneNumberVerificationFlowHandler: VerificationFlowHandler
    {
        private readonly VerificationCodePageArgsFactory _codePageArgsFactory;

        public EmailPhoneNumberVerificationFlowHandler(
            VerificationMethodUpdateFlowModel verificationMethodUpdateFlowModel, AccountVerificationService accountVerificationService, PageManager pageManager,
            VerificationCodePageArgsFactory codePageArgsFactory) : base(verificationMethodUpdateFlowModel, accountVerificationService, pageManager)
        {
            _codePageArgsFactory = codePageArgsFactory;
        }

        public override async Task<VerificationResult> VerifyCredentialsAsync()
        {
                var type = UserVerificationMethod.Type is CredentialType.Email ? VerifiableCredentialType.Email : VerifiableCredentialType .PhoneNumber;
                var result = await AccountVerificationService.SendVerificationCodeAsync(type, UserVerificationMethod.Input, false);
                
                return result.IsError ? new VerificationResult(result.ErrorCode, result.ErrorMessage) : new VerificationResult();
        }

        public override void MoveNext()
        {
            MoveToSendCodePage();
        }
        
        private void MoveToSendCodePage()
        {
            var pageArgs = _codePageArgsFactory.CreatePageArgs(UserVerificationMethod, false);

            pageArgs.MoveNextRequested = MoveNextRequested; 
            
            PageManager.MoveNext(pageArgs);

            async void MoveNextRequested()
            {
                try
                {
                    var verifyCredentials = await AccountVerificationService.VerifyCredentialsAsync(UserVerificationMethod);
                    if (verifyCredentials.IsError)
                    {
                        pageArgs.MoveNextFailed?.Invoke();
                        return;
                    }

                    var verifyUserResponse = verifyCredentials.Model;
                    if (!verifyUserResponse.IsSuccessful)
                    {
                        pageArgs.MoveNextFailed?.Invoke();
                        return;
                    }

                    TargetVerificationMethod.VerificationToken = verifyCredentials.Model.VerificationToken;

                    switch (OperationType)
                    {
                        case VerificationMethodOperationType.Change:
                            MoveToAddVerificationPage();
                            break;
                        case VerificationMethodOperationType.Remove:
                            await RemoveVerificationMethodAsync(pageArgs);
                            break;
                        default:
                            throw new InvalidEnumArgumentException();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            void MoveToAddVerificationPage()
            {
                PageManager.MoveNext(new VerificationMethodUpdatePageArgs(TargetVerificationMethod, VerificationMethodOperationType.Change));
            }
        }

        private async Task RemoveVerificationMethodAsync(VerificationCodeArgs pageArgs)
        {
            try
            {
                var result = await AccountVerificationService.RemoveVerificationMethodAsync(TargetVerificationMethod);
                if (result.IsError)
                {
                    pageArgs.MoveNextFailed?.Invoke();
                    return;
                }
                
                var methodUpdateResult = new VerificationMethodUpdateResult(TargetVerificationMethod.Type, VerificationMethodOperationType.Remove);
                
                Complete(methodUpdateResult);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
using System;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Modules.AccountVerification.Providers;
using Navigation.Core;

namespace UIManaging.Pages.AccountVerification.AccountConfirmation
{
    [UsedImplicitly]
    public sealed class VerificationFlowProvider
    {
        private readonly AccountVerificationService _accountVerificationService;
        private readonly PageManager _pageManager;
        private readonly VerificationCodePageArgsFactory _codePageArgsFactory;
        private readonly ICredentialsHandler _credentialsHandler;

        public VerificationFlowProvider(AccountVerificationService accountVerificationService, PageManager pageManager,
            VerificationCodePageArgsFactory codePageArgsFactory, ICredentialsHandler credentialsHandler)
        {
            _accountVerificationService = accountVerificationService;
            _pageManager = pageManager;
            _codePageArgsFactory = codePageArgsFactory;
            _credentialsHandler = credentialsHandler;
        }

        public VerificationFlowHandler GetFlowHandler(VerificationMethodUpdateFlowModel verificationMethodUpdateFlowModel)
        {
            switch (verificationMethodUpdateFlowModel.UserVerificationMethod.Type)
            {
                case CredentialType.Password:
                    return new PasswordVerificationFlowHandler(verificationMethodUpdateFlowModel, _accountVerificationService, _pageManager);
                case CredentialType.Email:
                case CredentialType.PhoneNumber:
                    return new EmailPhoneNumberVerificationFlowHandler(verificationMethodUpdateFlowModel, _accountVerificationService, _pageManager, _codePageArgsFactory);
                case CredentialType.AppleId:
                case CredentialType.GoogleId:
                    return new LinkableVerificationFlowHandler(verificationMethodUpdateFlowModel, _accountVerificationService, _pageManager, _credentialsHandler);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.Pages.OnBoardingPage.UI.Args;

namespace UIManaging.Pages.AccountVerification
{
    [UsedImplicitly]
    public sealed class VerificationCodePageArgsFactory
    {
        private readonly OnBoardingLocalization _onboardingLocalization;
        private readonly PageManager _pageManager;
        private readonly AccountVerificationService _accountVerificationService;

        public VerificationCodePageArgsFactory(OnBoardingLocalization onboardingLocalization, PageManager pageManager, AccountVerificationService accountVerificationService)
        {
            _onboardingLocalization = onboardingLocalization;
            _pageManager = pageManager;
            _accountVerificationService = accountVerificationService;
        }

        public VerificationCodeArgs CreatePageArgs(IVerificationMethod method, bool isNew)
        {
            var maskedCredentials = $"<b>{method.GetCredentialsMasked()}</b>";

            var pageArgs = new VerificationCodeArgs()
            {
                Description = $"{string.Format(GetFormattedDescription(method.Type), maskedCredentials)}",
                MoveBackRequested = MoveBack,
                NewVerificationCodeRequested = VerificationCodeRequested,
                OnValueChanged = OnValueChanged,
            };

            return pageArgs;

            void OnValueChanged(string value) => method.VerificationCode = value;
            void MoveBack() => _pageManager.MoveBack();

            async Task VerificationCodeRequested()
            {
                var type = method.Type == CredentialType.Email ? VerifiableCredentialType.Email : VerifiableCredentialType.PhoneNumber;
                
                await _accountVerificationService.SendVerificationCodeAsync(type, method.Input, isNew);
            }
        }

        private string GetFormattedDescription(CredentialType type)
        {
            switch (type)
            {
                case CredentialType.Email:
                return _onboardingLocalization.VerificationCodeEmailDescription;
                case CredentialType.PhoneNumber:
                return _onboardingLocalization.VerificationCodePhoneDescription;
                case CredentialType.AppleId:
                case CredentialType.Password:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using Modules.AccountVerification;
using Modules.AccountVerification.Events;
using Modules.AccountVerification.Providers;
using UnityEngine;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    internal sealed class AddLinkableLoginMethodHandler
    {
        private readonly AccountVerificationService _accountVerificationService;
        private readonly ICredentialsHandler _credentialsProvider;

        private CredentialType Type { get; }

        public AddLinkableLoginMethodHandler(AccountVerificationService accountVerificationService, ICredentialsHandler credentialsProvider)
        {
            _accountVerificationService = accountVerificationService;
            _credentialsProvider = credentialsProvider;

            Type = credentialsProvider.Type switch
            {
                LinkableCredentialType.Apple => CredentialType .AppleId,
                LinkableCredentialType.Google => CredentialType.GoogleId,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public async Task<VerificationMethodUpdateResult> LinkAsync()
        {
            var credentialsRequest = await _credentialsProvider.RequestCredentialsAsync();

            if (credentialsRequest.IsCanceled) return new VerificationMethodUpdateResult(true);

            if (credentialsRequest.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to request {Type}: {credentialsRequest.ErrorMessage}");
                return new VerificationMethodUpdateResult(Type, VerificationMethodOperationType.Add, false, credentialsRequest.ErrorMessage);
            }

            var method = new VerificationMethod(Type)
            {
                Input = credentialsRequest.AppleId,
                VerificationToken = credentialsRequest.AppleToken,
            };

            var addMethodResult = await _accountVerificationService.AddVerificationMethodAsync(method);
            if (addMethodResult.IsError)
            {
                var message = addMethodResult.ErrorCodeIsUsed() ? addMethodResult.ErrorMessage : null;
                return new VerificationMethodUpdateResult(Type, VerificationMethodOperationType.Add, false, message);
            }

            return new VerificationMethodUpdateResult(Type, VerificationMethodOperationType.Add);
        }
    }
}
using System;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using Modules.AccountVerification;

namespace UIManaging.Pages.AccountVerification
{
    [UsedImplicitly]
    public sealed class VerificationMethodPageArgsFactory
    {
        private readonly VerificationMethodsProvider _verificationMethodsProvider;

        public VerificationMethodPageArgsFactory(VerificationMethodsProvider verificationMethodsProvider)
        {
            _verificationMethodsProvider = verificationMethodsProvider;
        }

        public VerificationMethodUpdatePageArgs CreateAddPageArgs(CredentialType type, VerificationMethodOperationType operationType)
        {
            var method = _verificationMethodsProvider.GetVerificationMethod(type);
            
            if (method == null)
            {
                throw new ArgumentNullException($"Verification method of type {type} is not found");
            }
            
            return new VerificationMethodUpdatePageArgs(method, operationType);
        }
    }
}
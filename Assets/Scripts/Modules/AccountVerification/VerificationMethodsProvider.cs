using System.Collections.Generic;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;

namespace Modules.AccountVerification
{
    [UsedImplicitly]
    public class VerificationMethodsProvider
    {
        private readonly Dictionary<CredentialType, IVerificationMethod> _verificationMethodsMap;

        public VerificationMethodsProvider(AccountVerificationService service)
        {
            _verificationMethodsMap = new Dictionary<CredentialType, IVerificationMethod>();
        }

        public IVerificationMethod GetVerificationMethod(CredentialType type)
        {
            if (_verificationMethodsMap.TryGetValue(type, out var method)) return method;

            method = new VerificationMethod(type);
            
            _verificationMethodsMap.Add(type, method);

            return method;
        }
    }
}
using System;
using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification
{
    public interface IVerificationMethod
    {
        CredentialType Type { get; }
        string Input { get; set; }
        string VerificationCode { get; set; }
        string VerificationToken { get; set; }
        
        event Action<bool> InputValidated;
    }
}

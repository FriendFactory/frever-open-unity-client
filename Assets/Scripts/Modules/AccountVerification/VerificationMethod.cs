using System;
using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification
{
    public class VerificationMethod: IVerificationMethod
    {
        public CredentialType Type { get; }
        public string VerificationCode { get; set; }
        public string VerificationToken { get; set; }

        public string Input
        {
            get => _input;
            set
            {
                _input = value;
                InputValidated?.Invoke(this.IsInputValid());
            }
        }

        private string _input;

        public VerificationMethod(CredentialType type)
        {
            Type = type;
        }

        public event Action<bool> InputValidated;
    }
}
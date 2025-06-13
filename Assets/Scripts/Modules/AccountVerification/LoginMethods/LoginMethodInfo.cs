using System;
using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification.LoginMethods
{
    public sealed class LoginMethodInfo : ILoginMethodInfo
    {
        public CredentialType Type { get; }
        public CredentialStatus Status { get; }
        public bool IsLinked { get; }
        public bool IsChangeable { get; }

        public LoginMethodInfo(VerifiableCredentialType type, CredentialStatus credentialStatus)
        {
            Status = credentialStatus;
            Type = type == VerifiableCredentialType.Email ? CredentialType.Email : CredentialType.PhoneNumber;
            var credential = Type == CredentialType.Email ? credentialStatus.Email : credentialStatus.PhoneNumber;
            IsLinked = !string.IsNullOrEmpty(credential);
            IsChangeable = true;
        }

        public LoginMethodInfo(CredentialStatus credentialStatus)
        {
            Type = CredentialType.Password;
            Status = credentialStatus;
            IsLinked = Status.HasPassword;
            IsChangeable = true;
        }

        public LoginMethodInfo(LinkableCredentialType type, CredentialStatus credentialStatus)
        {
            Type = type switch
            {
                LinkableCredentialType.Apple => CredentialType.AppleId,
                LinkableCredentialType.Google => CredentialType.GoogleId,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            Status = credentialStatus;

            (IsLinked, IsChangeable) = type switch
            {
                LinkableCredentialType.Apple => (Status.HasAppleId, !Status.HasAppleId),
                LinkableCredentialType.Google => (Status.HasGoogleId, !Status.HasGoogleId),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
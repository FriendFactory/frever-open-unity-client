using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification.LoginMethods
{
    public interface ILoginMethodInfo
    {
        CredentialType Type { get; }
        CredentialStatus Status { get; }
        bool IsLinked { get; }
        bool IsChangeable { get; }
    }
}
using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification
{
    public static class CredentialTypeExtensions
    {
        public static bool IsLinkable(this CredentialType type) => type is CredentialType.AppleId or CredentialType.GoogleId;

        public static bool TryConvertToLinkableType(this CredentialType type, out LinkableCredentialType linkableType)
        {
            linkableType = LinkableCredentialType.Apple;
            
            if (!type.IsLinkable()) return false;

            linkableType = type is CredentialType.AppleId ? LinkableCredentialType.Apple : LinkableCredentialType.Google; 
            
            return true;
        }
    }
}
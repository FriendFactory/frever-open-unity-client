using System;
using SA.Foundation.Templates;
using SA.iOS.AuthenticationServices;

namespace Modules.AccountVerification.Providers
{
    internal sealed class AuthorizationDelegate : ISN_IASAuthorizationControllerDelegate
    {
        private readonly Action<ISN_ASAuthorizationAppleIDCredential> _onComplete;
        private readonly Action<SA_Error> _onFail;
        
        public AuthorizationDelegate(Action<ISN_ASAuthorizationAppleIDCredential> onComplete, Action<SA_Error> onFail)
        {
            _onComplete = onComplete;
            _onFail = onFail;
        }
        
        public void DidCompleteWithAuthorization(ISN_ASAuthorizationAppleIDCredential credential)
        {
            _onComplete?.Invoke(credential);
        }

        public void DidCompleteWithError(SA_Error error)
        {
            _onFail?.Invoke(error);
        }
    }
}
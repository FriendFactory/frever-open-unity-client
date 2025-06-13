using System;
using SA.Foundation.Templates;
using SA.iOS.AuthenticationServices;

namespace Modules.SignUp
{
    internal sealed class AuthorizationDelegate : ISN_IASAuthorizationControllerDelegate
    {
        private readonly Action<ISN_ASAuthorizationAppleIDCredential> _onComplete;
        private readonly Action<string> _onFail;
        
        public AuthorizationDelegate(Action<ISN_ASAuthorizationAppleIDCredential> onComplete, Action<string> onFail)
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
            _onFail?.Invoke(error.Message);
        }
    }
}
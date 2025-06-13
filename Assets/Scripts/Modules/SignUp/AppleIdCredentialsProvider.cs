using System;
using System.Collections.Generic;
using Bridge.Authorization.Models;
using SA.iOS.AuthenticationServices;

namespace Modules.SignUp
{
    public sealed class AppleIdCredentialsProvider
    {
        private ISN_ASAuthorizationAppleIDProvider _provider;
        private AppleAuthCredentials _credentials;
        
        public AppleIdCredentialsProvider()
        {
            _provider = new ISN_ASAuthorizationAppleIDProvider();
        }

        public event Action<AppleAuthCredentials> CredentialsRequestCompleted;
        public event Action CredentialsRequestFailed;

        public void RequestCredentials()
        {
            var request = _provider.CreateRequest();
            request.SetRequestedScopes(new List<ISN_ASAuthorizationScope>()
            {
                ISN_ASAuthorizationScope.Email
            });
            var requests = new ISN_ASAuthorizationRequest[] { request };
            var authorizationController = new ISN_ASAuthorizationController(requests);

            var @delegate = new AuthorizationDelegate(OnAuthorizationSuccess, OnAuthorizationFailed);
            authorizationController.SetDelegate(@delegate);
            authorizationController.PerformRequests();
        }

        private void OnAuthorizationSuccess(ISN_ASAuthorizationAppleIDCredential appleIdCredential)
        {
            _credentials = new AppleAuthCredentials();
            var identityToken = Convert.ToBase64String(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
            _credentials.Email = appleIdCredential.Email;
            _credentials.AppleId = appleIdCredential.User;
            _credentials.AppleIdentityToken = identityToken;
            
            CredentialsRequestCompleted?.Invoke(_credentials);
        }
        
        private void OnAuthorizationFailed(string message)
        {
            if (_credentials != null)
            {
                CredentialsRequestCompleted?.Invoke(_credentials);
            }
            else
            {
                CredentialsRequestFailed?.Invoke();
            }
        }
    }
}
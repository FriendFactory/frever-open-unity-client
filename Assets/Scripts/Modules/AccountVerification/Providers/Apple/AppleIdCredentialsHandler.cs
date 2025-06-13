using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using SA.Foundation.Templates;
using SA.iOS.AuthenticationServices;
using UnityEngine;

namespace Modules.AccountVerification.Providers
{
    [UsedImplicitly]
    public sealed class AppleIdCredentialsHandler: ICredentialsHandler
    {
        private const float APPLE_ID_REQUEST_TIMEOUT = 30f;
        
        private ISN_ASAuthorizationAppleIDProvider _provider;
        
        private CredentialsRequestResult _lastResult;

        public LinkableCredentialType Type => LinkableCredentialType.Apple;

        private bool IsRequesting { get; set; }

        public async Task<CredentialsRequestResult> RequestCredentialsAsync()
        {
            _provider ??= new ISN_ASAuthorizationAppleIDProvider();

            IsRequesting = true;
            
            RequestCredentials();

            var timeout = Time.time + APPLE_ID_REQUEST_TIMEOUT;
            while (_lastResult == null)
            {
                if (Time.time > timeout)
                {
                    _lastResult = new CredentialsRequestResult();
                    break;
                }

                await Task.Delay(42);
            }

            IsRequesting = false;

            return _lastResult;
        }

        private void RequestCredentials()
        {
            _lastResult = null;
            
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
            if (!IsRequesting) return;
            
            var userId = appleIdCredential.User;
            var identityToken = Convert.ToBase64String(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
            
            _lastResult = new CredentialsRequestResult(userId, identityToken);
        }
        
        private void OnAuthorizationFailed(SA_Error error)
        {
            if (!IsRequesting) return;
            
            if (error.Code == 1001)
            {
                _lastResult = new CredentialsRequestResult();
                return;
            }
            
            Debug.LogError($"[{GetType().Name}] Apple Id Authorization failed: {error.FullMessage}");
            
            _lastResult = new CredentialsRequestResult(error.Message);
        }
    }
}
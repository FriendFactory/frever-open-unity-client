using System;
using Bridge.Authorization.Models;
using Common;
using SA.Android.GMS.Auth;
using SA.Android.GMS.Common;
using SA.Foundation.Templates;
using UnityEngine;

namespace Modules.SignUp
{
    public sealed class GooglePlayCredentialsProvider
    {
        
        private GoogleAuthCredentials _credentials;
        
        public event Action<GoogleAuthCredentials> CredentialsRequestCompleted;
        public event Action<SA_Error> CredentialsRequestFailed;

        public void RequestCredentials()
        {
            var result = AN_GoogleApiAvailability.IsGooglePlayServicesAvailable();
            
            if (result != AN_ConnectionResult.SUCCESS) 
            {
                AN_GoogleApiAvailability.MakeGooglePlayServicesAvailable(resolution => 
                {
                    if (resolution.IsSucceeded) 
                    {
                        GoogleSignIn();
                    } 
                    else
                    {
                        OnAuthorizationFailed(resolution.Error);
                    }
                });
            } 
            else 
            {
                GoogleSignIn();
            }
        }

        private void GoogleSignIn()
        {
            var builder = new AN_GoogleSignInOptions.Builder(AN_GoogleSignInOptions.DEFAULT_SIGN_IN);

            builder.RequestEmail();
            builder.RequestId();
            builder.RequestIdToken(Constants.GOOGLE_SERVER_CLIENT_ID);
            
            var gso = builder.Build();
            var client = AN_GoogleSignIn.GetClient(gso);
            
            client.SilentSignIn(result => 
            {
                if (result.IsSucceeded) 
                {
                    OnAuthorizationSuccess(result.Account);
                } 
                else 
                {
                    Debug.Log($"SilentSignIn failed with code: {result.Error.Code}");
                    
                    client.SignIn(signInResult => 
                    {
                        if (signInResult.IsSucceeded) 
                        {
                            OnAuthorizationSuccess(signInResult.Account);
                        } 
                        else 
                        {
                            OnAuthorizationFailed(signInResult.Error);
                        }
                    });
                }
            });
        }

        private void OnAuthorizationSuccess(AN_GoogleSignInAccount googleAccount)
        {
            Debug.Log("Successful login");
            Debug.Log("ID: " + googleAccount.GetId());
            Debug.Log("ID Token: " + googleAccount.GetIdToken());

            _credentials = new GoogleAuthCredentials
            {
                Email = googleAccount.GetEmail(),
                GoogleId = googleAccount.GetId(),
                GoogleIdentityToken = googleAccount.GetIdToken()
            };

            CredentialsRequestCompleted?.Invoke(_credentials);
        }
        
        private void OnAuthorizationFailed(SA_Error error)
        {
            if (_credentials != null)
            {
                CredentialsRequestCompleted?.Invoke(_credentials);
            }
            else
            {
                CredentialsRequestFailed?.Invoke(error);
            }
        }
    }
}
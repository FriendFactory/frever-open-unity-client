using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UserLoginManagement;
using UnityEngine;
using Zenject;

namespace Modules.AccountVerification.LoginMethods
{
    [UsedImplicitly]
    public sealed class LoginMethodsProvider: IInitializable, IDisposable
    {
        private readonly AccountVerificationService _service;
        private readonly UserAccountManager _userAccountManager;

        private CredentialStatus _credentialStatus;
        private List<ILoginMethodInfo> _loginMethods;
        private CancellationTokenSource _tokenSource;
        
        public IEnumerable<ILoginMethodInfo> LoginMethods => _loginMethods;
        public bool Initialized => _loginMethods != null;

        public event Action LoginMethodsUpdated;

        public LoginMethodsProvider(AccountVerificationService service, UserAccountManager userAccountManager)
        {
            _service = service;
            _userAccountManager = userAccountManager;
        }

        public void Initialize()
        {
            _userAccountManager.OnUserLoggedIn += OnUserLoggedIn;
            _userAccountManager.OnUserLoggedOut += OnUserLoggedOut;
        }

        public void Dispose()
        {
            _userAccountManager.OnUserLoggedIn -= OnUserLoggedIn;
            _userAccountManager.OnUserLoggedOut -= OnUserLoggedOut;
            
            Cancel();
        }

        public async Task UpdateLoginMethodsAsync()
        {
            try
            {
                Cancel();

                _tokenSource = new CancellationTokenSource();
                _credentialStatus = await _service.GetCredentialStatusAsync(_tokenSource.Token);

                if (_credentialStatus == null) throw new ArgumentNullException(nameof(_credentialStatus));

                GenerateLoginMethods(_credentialStatus);

                LoginMethodsUpdated?.Invoke();
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public bool IsLinked(CredentialType type)
        {
            if (_credentialStatus == null) return false;
            
            switch (type)
            {
                case CredentialType.Email:
                    return !string.IsNullOrEmpty(_credentialStatus.Email);
                case CredentialType.PhoneNumber:
                    return !string.IsNullOrEmpty(_credentialStatus.PhoneNumber);
                case CredentialType.Password:
                    return _credentialStatus.HasPassword;
                case CredentialType.AppleId:
                    return _credentialStatus.HasAppleId;
                case CredentialType.GoogleId:
                    return _credentialStatus.HasGoogleId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        private void GenerateLoginMethods(CredentialStatus credentialStatus)
        {
            _loginMethods = new List<ILoginMethodInfo>
            {
                new LoginMethodInfo(VerifiableCredentialType.Email, credentialStatus),
                new LoginMethodInfo(VerifiableCredentialType.PhoneNumber, credentialStatus),
                new LoginMethodInfo(credentialStatus),
        #if UNITY_IOS && !UNITY_EDITOR
                new LoginMethodInfo(LinkableCredentialType.Apple, credentialStatus),
        #elif UNITY_ANDROID && !UNITY_EDITOR
                new LoginMethodInfo(LinkableCredentialType.Google, credentialStatus),
        #endif
            };
        }

        private void Cancel()
        {
            if (_tokenSource == null) return;
            
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;
        }

        private async void OnUserLoggedIn()
        {
            try
            {
                _userAccountManager.OnUserLoggedIn -= OnUserLoggedIn;
            
                await UpdateLoginMethodsAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnUserLoggedOut()
        {
            _userAccountManager.OnUserLoggedIn += OnUserLoggedIn;
        }
    }
}
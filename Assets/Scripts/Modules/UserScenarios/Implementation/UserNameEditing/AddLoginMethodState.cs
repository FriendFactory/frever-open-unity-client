using Bridge.Authorization.Models;
using Extensions;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Modules.AccountVerification.LoginMethods;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using UIManaging.Pages.OnBoardingPage;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.UserNameEditing
{
    [UsedImplicitly]
    internal sealed class AddLoginMethodState : StateBase<EditNameContext>, IResolvable
    {
        private readonly PopupManager _popupManager;
        private readonly ISignUpService _signUpService;
        private readonly AccountVerificationService _accountVerificationService;

        private AddLoginMethodConfiguration _configs;
        
        public ITransition MoveNext;
        public ITransition MoveBack;
        public ITransition MoveNextNoVerification;

        public AddLoginMethodState(PopupManager popupManager, ISignUpService signUpService, AccountVerificationService accountVerificationService)
        {
            _popupManager = popupManager;
            _signUpService = signUpService;
            _accountVerificationService = accountVerificationService;
        }

        public override ScenarioState Type => ScenarioState.AddLoginMethod;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack, MoveNextNoVerification}.RemoveNulls();

        public override void Run()
        {
            _configs = new AddLoginMethodConfiguration(Context.SelectedName);
            _configs.ValidCredentialsProvided += OnValidCredentialsProvided;
            _configs.MoveNextFailed += OnMoveNextFailed;
            _configs.OnComplete += OnPopupComplete;

            _popupManager.SetupPopup(_configs);
            _popupManager.ShowPopup(_configs.PopupType);
        }

        private async void OnValidCredentialsProvided(ICredentials credentials)
        {
            Context.Credentials = credentials;
            _signUpService.SetCredentials(credentials);
            
            if (credentials is AppleAuthCredentials or GoogleAuthCredentials or UsernameAndPasswordCredentials)
            {
                _signUpService.SetUserName(Context.OriginalName);

                var model = _accountVerificationService.ToVerificationMethodModel(credentials);
                var result = await _accountVerificationService.AddVerificationMethodAsync(model);

                if (!result.IsError)
                {
                    ClearEvents();
                    OnPopupComplete(AddLoginMethodResult.Close);
                    Context.LoginMethodUpdated = true;
                    await MoveNextNoVerification.Run();
                }
                else
                {
                    ClearEvents();
                    OnPopupComplete(AddLoginMethodResult.Close);
                    Debug.LogError(result.ErrorMessage);
                    _configs.MoveNextFailed?.Invoke();
                }
            }
            else
            {
                await _signUpService.RequestVerificationCode();
            
                ClearEvents();
                OnPopupComplete(AddLoginMethodResult.Close);
                Context.LoginMethodUpdated = true;
                
                await MoveNext.Run();
            }
        }

        private async void OnMoveNextFailed()
        {
            Debug.LogError("Failed to add login method");
            OnPopupComplete(AddLoginMethodResult.Close);
            await MoveBack.Run();
        }
        
        private void OnPopupComplete(AddLoginMethodResult result)
        {
            switch (result)
            {
                case AddLoginMethodResult.Close:
                    _popupManager.ClosePopupByType(PopupType.AddLoginMethod);
                    break;
                case AddLoginMethodResult.Next:
                    // Handle successful login method addition if needed
                    break;
            }
        }
        
        private void ClearEvents()
        {
            _configs.ValidCredentialsProvided -= OnValidCredentialsProvided;
            _configs.MoveNextFailed -= OnMoveNextFailed;
        }
    }
}
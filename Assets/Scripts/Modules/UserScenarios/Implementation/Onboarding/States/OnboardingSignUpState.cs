using Bridge.Authorization.Models;
using Extensions;
using JetBrains.Annotations;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingSignUpState : StateBase<ISignupContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly ISignUpService _signUpService;
        private readonly ISignInService _signInService;
        private readonly LocalUserDataHolder _localUser;

        private SignUpArgs _args;

        public ITransition MoveBack;
        public ITransition MoveNext;
        public ITransition MoveNextNoVerification;
        public ITransition MoveSignIn;
        public ITransition MoveSignInNoVerification;

        public OnboardingSignUpState(PageManager pageManager, ISignUpService signUpService, ISignInService signInService, LocalUserDataHolder localUser)
        {
            _pageManager = pageManager;
            _signUpService = signUpService;
            _signInService = signInService;
            _localUser = localUser;
        }
        
        public override ScenarioState Type => ScenarioState.SignUpGeneral;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext, MoveNextNoVerification, 
            MoveSignIn, MoveSignInNoVerification }.RemoveNulls();
        
        public override void Run()
        {
            _args = new SignUpArgs();
            _args.ValidCredentialsProvided += OnValidCredentialsProvided;
            _args.MoveBackRequested += OnMoveBackRequested;
            _args.MoveToSignInRequested += OnMoveToSignInRequested;
            _pageManager.MoveNext(_args);
        }

        private async void OnValidCredentialsProvided(ICredentials credentials)
        {
            _signUpService.SetCredentials(credentials);
            Context.Credentials = credentials;

            if (credentials is AppleAuthCredentials or GoogleAuthCredentials or UsernameAndPasswordCredentials)
            {
                var result = await _signUpService.SaveUserInfo();
                    
                if (result.IsSuccess)
                {
                    ClearEvents();
                    
                    await MoveNextNoVerification.Run();
                }
                else if (credentials is AppleAuthCredentials or GoogleAuthCredentials)
                {
                    _signInService.SetCredentials(credentials);
                    
                    var loginResult = await _signInService.LoginUser();
                
                    if (loginResult.IsError)
                    {
                        Debug.LogError(loginResult.ErrorMessage);
                        _args.MoveNextFailed?.Invoke();
                        return;
                    }
                    
                    if (_localUser.UserProfile == null)
                    {
                        await _localUser.DownloadProfile();
                    }
                    
                    ClearEvents();
                
                    await MoveSignInNoVerification.Run();
                }
                else
                {
                    ClearEvents();
                    Debug.LogError(result.ErrorMessage);
                    _args.MoveNextFailed?.Invoke();
                }
            }
            else
            {
                await _signUpService.RequestVerificationCode();
            
                ClearEvents();
                
                await MoveNext.Run();
            }
        }

        private async void OnMoveBackRequested()
        {
            ClearEvents();
            
            await MoveBack.Run();
        }

        private async void OnMoveToSignInRequested(ICredentials credentials)
        {
            ClearEvents();
            
            _signUpService.SetCredentials(credentials);
            Context.Credentials = credentials;

            await MoveSignIn.Run();
        }

        private void ClearEvents()
        {
            _args.ValidCredentialsProvided -= OnValidCredentialsProvided;
            _args.MoveBackRequested -= OnMoveBackRequested;
            _args.MoveToSignInRequested -= OnMoveToSignInRequested;
        }
    }
}
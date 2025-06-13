using Bridge.Authorization.Models;
using Extensions;
using JetBrains.Annotations;
using Modules.AccountVerification;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.UserNameEditing.States
{
    [UsedImplicitly]
    internal sealed class SetupAuthState: StateBase<EditNameContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly ISignUpService _signUpService;
        private readonly AccountVerificationService _accountVerificationService;

        private SignUpArgs _args;

        public ITransition MoveNext;
        public ITransition MoveNextNoVerification;

        public SetupAuthState(PageManager pageManager, ISignUpService signUpService, AccountVerificationService accountVerificationService)
        {
            _pageManager = pageManager;
            _signUpService = signUpService;
            _accountVerificationService = accountVerificationService;
        }
        
        public override ScenarioState Type => ScenarioState.SetupAuthentication;
        public override ITransition[] Transitions => new[] { MoveNext, MoveNextNoVerification }.RemoveNulls();
        
        public override void Run()
        {
            _args = new SignUpArgs();
            _args.ValidCredentialsProvided += OnValidCredentialsProvided;
            _args.MoveBackRequested += OnMoveBackRequested;
            _pageManager.MoveNext(_args);
        }

        private async void OnValidCredentialsProvided(ICredentials credentials)
        {
            _signUpService.SetCredentials(credentials);
            Context.Credentials = credentials;

            if (credentials is AppleAuthCredentials or GoogleAuthCredentials or UsernameAndPasswordCredentials)
            {
                _signUpService.SetUserName(Context.OriginalName);

                var model = _accountVerificationService.ToVerificationMethodModel(credentials);
                var result = await _accountVerificationService.AddVerificationMethodAsync(model);

                if (!result.IsError)
                {
                    ClearEvents();
                    await MoveNextNoVerification.Run();
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

        private void OnMoveBackRequested()
        {
            ClearEvents();
            
            _pageManager.MoveBack();
        }

        private void ClearEvents()
        {
            _args.ValidCredentialsProvided -= OnValidCredentialsProvided;
            _args.MoveBackRequested -= OnMoveBackRequested;
        }
    }
}
using Extensions;
using JetBrains.Annotations;
using Modules.Amplitude;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.Pages.OnBoardingPage.UI.Args;
using System.Threading.Tasks;
using Modules.AccountVerification;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.UserNameEditing.States
{
    [UsedImplicitly]
    internal sealed class VerificationState: StateBase<EditNameContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly ISignUpService _signUpService;
        private readonly OnBoardingLocalization _localization;
        private readonly AccountVerificationService _accountVerificationService;
        private readonly AmplitudeManager _amplitudeManager;

        private VerificationCodeArgs _pageArgs;

        public ITransition MoveNext;
        public ITransition MoveBack;

        public VerificationState(PageManager pageManager, ISignUpService signUpService, 
            OnBoardingLocalization localization, AccountVerificationService accountVerificationService, 
            AmplitudeManager amplitudeManager)
        {
            _pageManager = pageManager;
            _signUpService = signUpService;
            _localization = localization;
            _accountVerificationService = accountVerificationService;
            _amplitudeManager = amplitudeManager;
        }

        public override ScenarioState Type => ScenarioState.VerificationCode;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();
        
        public override void Run()
        {
            _pageArgs = new VerificationCodeArgs
            {
                Description = Description(),
                MoveNextRequested = OnMoveNextRequested,
                OnValueChanged = OnInputChanged,
                MoveBackRequested = OnMoveBackRequested,
                NewVerificationCodeRequested = NewVerificationCodeRequested,
            };
            _pageManager.MoveNext(_pageArgs);
        }

        private async void OnMoveBackRequested()
        {
            await MoveBack.Run();
        }

        private void OnInputChanged(string value)
        {
            _signUpService.SetVerificationCode(value);
        }

        private async void OnMoveNextRequested()
        {
            _amplitudeManager.LogEvent(_signUpService.IsPhoneSignup()
                                            ? AmplitudeEventConstants.EventNames.CONFIRM_PHONE_CODE
                                            : AmplitudeEventConstants.EventNames.CONFIRM_EMAIL_CODE);

            var model = _accountVerificationService.ToVerificationMethodModel(Context.Credentials);
            var result = await _accountVerificationService.AddVerificationMethodAsync(model);
            if (!result.IsError)
            {
                Context.LoginMethodUpdated = true;
                await MoveNext.Run();
                return;
            }

            _pageArgs.MoveNextFailed?.Invoke();
        }

        private Task NewVerificationCodeRequested()
        {
            return _signUpService.RequestVerificationCode();
        }

        private string Description()
        {
            var format = _signUpService.IsPhoneSignup() 
                ? _localization.VerificationCodePhoneDescription 
                : _localization.VerificationCodeEmailDescription;
            return string.Format(format, _signUpService.GetLogin());
        }
    }
}
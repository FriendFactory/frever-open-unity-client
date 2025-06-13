using System.Threading.Tasks;
using Bridge;
using JetBrains.Annotations;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.Onboarding
{
    [UsedImplicitly]
    internal sealed class FinishOnboardingTransition : TransitionBase<ISignupContext>, IResolvable
    {
        private readonly IBridge _bridge;
        private readonly IDataFetcher _dataFetcher;
        private readonly ISignUpService _signUpService;
        private readonly AmplitudeManager _amplitudeManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override ScenarioState To => ScenarioState.PostOnboardingExit;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public FinishOnboardingTransition(IBridge bridge, IDataFetcher dataFetcher, ISignUpService signUpService, AmplitudeManager amplitudeManager)
        {
            _bridge = bridge;
            _dataFetcher = dataFetcher;
            _signUpService = signUpService;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            var result = await _signUpService.SaveUserInfo();
            if (result.IsSuccess)
            {
                _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.USER_REGISTERED);
            }
            else
            {
                Debug.LogError($"Failed to register. Reason: {result.ErrorMessage}");
                return;
            }
            
            await _dataFetcher.FetchSeason();

            result = await _bridge.CompleteOnboarding();

            if (result.IsError)
            {
                Debug.LogError($"Failed to mark onboarding as completed, reason: {result.ErrorMessage}");
            }
            
            await base.OnRunning();
        }
    }
}
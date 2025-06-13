using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding.Transitions
{
    [UsedImplicitly]
    internal sealed class GoToContactsStateTransition : TransitionBase<ISignupContext>, IResolvable
    {
        [Inject] private ISignUpService _signUpService;
        [Inject] private IDataFetcher _dataFetcher;
        public override ScenarioState To => ScenarioState.OnboardingContactsFollow;
        
        protected override async Task UpdateContext()
        {
            await _dataFetcher.FetchSeason();
        }
    }
}
using System.Threading.Tasks;
using Bridge;
using Common;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.SignUp;
using Modules.TempSaves.Manager;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding.Transitions
{
    [UsedImplicitly]
    internal sealed class SignInCompletedTransition : TransitionBase<ICharacterCreationContext>, IResolvable
    {
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private TempFileManager _tempFileManager;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private IBridge _bridge;

        public override ScenarioState To => ScenarioState.PostOnboardingExit;
        
        protected override Task UpdateContext()
        {
            Context.IsNewCharacter = true;
            return Task.CompletedTask;
        }
        
        protected override async Task OnRunning()
        {
            await base.OnRunning();
            _dataFetcher.ResetData();
        }
    }
}
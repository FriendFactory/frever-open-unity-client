using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.SignUp;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.OnBoardingPage.UI;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingBackToCharacterEditorTransition : TransitionBase<OnboardingContext>, IResolvable
    {
        [Inject] private ISignUpService _signUpService;
        [Inject] private LocalUserDataHolder _localUser;
        
        public override ScenarioState To => ScenarioState.OnboardingCharacterEditor;
        
        protected override async Task UpdateContext()
        {
            Context.IsNewCharacter = false;
            Context.Character = Context.CharacterEditor.Character;

            await _localUser.UpdateBalance();
        }
    }
}
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingAvatarPreviewToCharacterEditorTransition : TransitionBase<ICharacterCreationContext>, IResolvable
    {
        [Inject] private LocalUserDataHolder _localUser;
        
        public override ScenarioState To => ScenarioState.OnboardingCharacterEditor;
        
        protected override async Task UpdateContext()
        {
            Context.IsNewCharacter = true;

            await _localUser.UpdateBalance();
        }
    }
}
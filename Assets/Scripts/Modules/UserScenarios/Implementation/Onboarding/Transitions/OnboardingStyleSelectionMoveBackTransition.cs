using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;

namespace Modules.UserScenarios.Implementation.Onboarding.Transitions
{
    [UsedImplicitly]
    internal sealed class OnboardingStyleSelectionMoveBackTransition: TransitionBase<ICharacterCreationContext>
    {
        public override ScenarioState To { get; }

        public OnboardingStyleSelectionMoveBackTransition(ScenarioState toState)
        {
            To = toState;
        }

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            Context.Style = null;
            
            await base.OnRunning();
        }
    }
}
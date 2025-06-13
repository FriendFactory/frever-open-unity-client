using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class OnBoardingCreateNewCharacterScenario : CharacterCreationScenarioBase, ICreateNewCharacterOnBoardingScenario
    {
        public OnBoardingCreateNewCharacterScenario(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, ScenarioState.RaceSelection);
        }
        
        private sealed class EntryTransition: EntryTransitionBase<ICharacterCreationContext>
        {
            public override ScenarioState To { get; }

            public EntryTransition(ScenarioArgsBase args, ScenarioState to) : base(args)
            {
                To = to;
            }

            protected override Task UpdateContext()
            {
                Context.IsNewCharacter = true;
                Context.AllowBackFromGenderSelection = false;
                return base.UpdateContext();
            }
        }
    }
}
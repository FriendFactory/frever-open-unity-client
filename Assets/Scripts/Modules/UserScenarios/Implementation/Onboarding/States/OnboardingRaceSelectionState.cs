using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingRaceSelectionState : StateBase<ICharacterCreationContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        
        public ITransition MoveNext;
        
        public override ScenarioState Type => ScenarioState.RaceSelection;
        public override ITransition[] Transitions => new[] { MoveNext }.RemoveNulls();

        public OnboardingRaceSelectionState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            var raceSelectionPageArgs = new RaceSelectionPageArs
            {
                RaceSelected = OnMoveNext
            };

            _pageManager.MoveNext(raceSelectionPageArgs);
        }

        private async void OnMoveNext(Race race)
        {
            Context.Race = race;
            await MoveNext.Run();
        }
    }
}
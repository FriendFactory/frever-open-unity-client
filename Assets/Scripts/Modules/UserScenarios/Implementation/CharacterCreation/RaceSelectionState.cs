using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class RaceSelectionState: StateBase<ICharacterCreationContext>, IResolvable
    {
        public override ScenarioState Type => ScenarioState.RaceSelection;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext }.RemoveNulls();

        public ITransition MoveNext;
        public ITransition MoveBack;

        private readonly PageManager _pageManager;

        public RaceSelectionState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            var args = new RaceSelectionPageArs
            {
                RaceSelected = OnRaceSelected,
                MoveBackRequested = () => MoveBack.Run()
            };
            _pageManager.MoveNext(args);
        }

        private void OnRaceSelected(Race race)
        {
            Context.Race = race;
            MoveNext.Run();
        }
        
    }
}
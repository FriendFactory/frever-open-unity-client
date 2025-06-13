using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;

namespace Modules.UserScenarios.Implementation.Onboarding.Transitions
{
    internal sealed class StartCharacterCreationTransition: SwitchTransitionBase<ICharacterCreationContext>
    {
        private readonly IMetadataProvider _metadataProvider;

        public StartCharacterCreationTransition(IMetadataProvider metadataProvider, ITransition[] subTransitions) : base(subTransitions)
        {
            _metadataProvider = metadataProvider;
        }

        protected override Task UpdateContext()
        {
            var universesWithCharacterCreation = _metadataProvider.MetadataStartPack.Universes.Where(x=>x.Races.Any(r=> r.Settings.CanCreateCharacters)).ToArray();
            Destination = universesWithCharacterCreation.Length == 1
                ? ScenarioState.CombinedStyleSelection
                : ScenarioState.RaceSelection;
            Context.AllowBackFromGenderSelection = Destination == ScenarioState.RaceSelection;
            if (Destination != ScenarioState.RaceSelection)
            {
                var raceId = universesWithCharacterCreation.First().Races.First().RaceId;
                Context.Race = _metadataProvider.MetadataStartPack.GetRaces().First(x=> x.Id == raceId);
            }
            return Task.CompletedTask;
        }
    }
}
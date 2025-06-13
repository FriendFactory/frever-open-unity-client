using System.Linq;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using UnityEngine;
using Zenject;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class CreateNewCharacterScenario : CharacterCreationScenarioBase, ICreateNewCharacterScenario
    {
        public CreateNewCharacterScenario(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, Resolve<IMetadataProvider>());
        }
        
        private sealed class EntryTransition: EntryTransitionBase<ICharacterCreationContext>
        {
            private readonly IMetadataProvider _metadataProvider;
            private ScenarioState _to;
            public override ScenarioState To => _to;

            public EntryTransition(CreateNewCharacterArgs args, IMetadataProvider metadataProvider) : base(args)
            {
                _metadataProvider = metadataProvider;
            }

            protected override Task UpdateContext()
            {
                var createArgs = (CreateNewCharacterArgs)ScenarioArgs;
                if (createArgs.RaceId.HasValue)
                {
                    Context.Race = _metadataProvider.MetadataStartPack.GetRaces().First(x=> x.Id == createArgs.RaceId);
                    Context.RaceLocked = true;
                    _to = ScenarioState.CombinedStyleSelection;
                }
                else
                {
                    _to = ScenarioState.RaceSelection;
                }
                Context.IsNewCharacter = true;
                Context.AllowBackFromGenderSelection = true;
                Context.OnDisplayed = createArgs.OnDisplayed;
                return base.UpdateContext();
            }
        }
    }
}
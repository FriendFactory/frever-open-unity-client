using System.Threading.Tasks;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class CharacterStyleSelectionMoveBackTransition: TransitionBase<ICharacterCreationContext>, IResolvable
    {
        private ScenarioState _to;
        public override ScenarioState To => _to;
        
        protected override Task UpdateContext()
        {
            _to = Context.RaceLocked ? ScenarioState.PreviousPageExit : ScenarioState.RaceSelection;
            return Task.CompletedTask;
        }
    }
}
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class AvatarPreviewToSelfieTakingTransition : TransitionBase<ICharacterCreationContext>, IResolvable
    {
        public override ScenarioState To => ScenarioState.TakingSelfie;
        
        protected override Task UpdateContext()
        {
            Context.JsonSelfie = null;
            return Task.CompletedTask;
        }
    }
}
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class AvatarPreviewToCharacterEditorTransition : TransitionBase<ICharacterCreationContext>, IResolvable
    {
        public override ScenarioState To => ScenarioState.CharacterEditor;
        
        protected override Task UpdateContext()
        {
            Context.IsNewCharacter = true;
            return Task.CompletedTask;
        }
    }
}
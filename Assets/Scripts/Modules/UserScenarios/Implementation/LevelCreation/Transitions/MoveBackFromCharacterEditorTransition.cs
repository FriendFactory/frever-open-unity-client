using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class MoveBackFromCharacterEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState To => Context.CharacterEditor.OpenedFrom;
        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }
    }
}
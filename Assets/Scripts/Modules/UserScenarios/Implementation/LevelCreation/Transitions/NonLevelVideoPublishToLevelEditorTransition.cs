using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class NonLevelVideoPublishToLevelEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState To => ScenarioState.LevelEditor;

        protected override Task UpdateContext()
        {
            Context.LevelEditor.OpenVideoUploadMenu = true;
            return Task.CompletedTask;
        }
    }
}
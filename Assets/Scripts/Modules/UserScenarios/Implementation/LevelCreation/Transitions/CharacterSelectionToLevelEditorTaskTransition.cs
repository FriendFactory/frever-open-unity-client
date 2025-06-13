using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class CharacterSelectionToLevelEditorTaskTransition: CharacterSelectionToEditorTaskTransitionBase
    {
        public override ScenarioState To => ScenarioState.LevelEditor;
    }
}
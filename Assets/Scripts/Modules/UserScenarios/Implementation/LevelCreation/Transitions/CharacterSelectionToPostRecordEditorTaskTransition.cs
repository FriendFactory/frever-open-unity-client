using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class CharacterSelectionToPostRecordEditorTaskTransition: CharacterSelectionToEditorTaskTransitionBase
    {
        public override ScenarioState To => ScenarioState.PostRecordEditor;
    }
}
using JetBrains.Annotations;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class CharacterSelectionToCharacterEditorTaskTransition : CharacterSelectionToEditorTaskTransitionBase
    {
        public override ScenarioState To => ScenarioState.CharacterEditor;
    }
}
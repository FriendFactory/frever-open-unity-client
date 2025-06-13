using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Scenarios;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class CharacterSelectionToLevelEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly IUndressingCharacterService _undressingCharacterService;
        private readonly IBridge _bridge;

        public override ScenarioState To => ScenarioState.LevelEditor;

        public CharacterSelectionToLevelEditorTransition(IUndressingCharacterService undressingCharacterService, IBridge bridge)
        {
            _undressingCharacterService = undressingCharacterService;
            _bridge = bridge;
        }

        protected override Task UpdateContext()
        {
            if (Context.InitialTemplateId.HasValue)
            {
                Context.LevelEditor.CharactersToUseInTemplate = Context.CharacterSelection.PickedCharacters;
            }

            var targetId = Context.CharacterSelection.CharacterToReplaceIds.First();
            var focusedCharacter = Context.LevelEditor.CharactersToUseInTemplate[targetId];

            Context.LevelEditor.ShowDressingStep = focusedCharacter.GroupId == _bridge.Profile.GroupId;
            Context.LevelEditor.ShowLoadingPagePopup = true;
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            var pickedCharacters = Context.CharacterSelection.PickedCharacters;

            if (pickedCharacters != null && Context.LevelData != null)
            {
                Context.LevelData.ReplaceCharacters(pickedCharacters);
            }
            return base.OnRunning();
        }
    }
}
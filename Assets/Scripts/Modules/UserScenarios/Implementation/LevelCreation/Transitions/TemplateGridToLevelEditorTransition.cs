using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Models;
using Modules.CharacterManagement;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;
using UIManaging.Localization;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    internal sealed class TemplateGridToLevelEditorTransition : TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ITemplateProvider _templateProvider;
        private readonly CharacterManager _characterManager;

        [Inject] private FeedLocalization _localization;
        
        private ScenarioState _to;
        public override ScenarioState To => _to;

        public TemplateGridToLevelEditorTransition(ITemplateProvider templateProvider,
            CharacterManager characterManager)
        {
            _templateProvider = templateProvider;
            _characterManager = characterManager;
        }

        protected override async Task UpdateContext()
        {
            var templateEvent = await _templateProvider.GetTemplateEvent(Context.InitialTemplateId.Value);
            if (templateEvent == null)
            {
                throw new Exception($"Failed to download template: {Context.InitialTemplateId}");
            }

            var charactersToReplace = templateEvent.GetUniqueCharacterIds();
            Context.CharacterSelection.CharacterToReplaceIds = charactersToReplace;
            var characterCount = charactersToReplace.Length;
            var showCharacterSelection = characterCount > 1;

            if (showCharacterSelection)
            {
                Context.CharacterSelection.Header = _localization.TemplateSelectCharactersTitle;
                Context.CharacterSelection.HeaderDescription = string.Empty;
                Context.CharacterSelection.ReasonText = string.Format(_localization.TemplateSelectCharactersReasonFormat, characterCount);
                _to = ScenarioState.CharacterSelection;
            }
            else
            {
                _to = ScenarioState.LevelEditor;
                await SetMainCharacterAsPicked(templateEvent);
                Context.LevelEditor.CharactersToUseInTemplate = Context.CharacterSelection.PickedCharacters;
            }
            Context.LevelEditor.ShowLoadingPagePopup = true;
        }

        private async Task SetMainCharacterAsPicked(Event templateEvent)
        {
            Context.CharacterSelection.PickedCharacters = new Dictionary<long, CharacterFullInfo>();
            var replaceTarget = templateEvent.GetUniqueCharacterIds().First();
            Context.CharacterSelection.PickedCharacters[replaceTarget] =
                await _characterManager.GetSelectedCharacterFullInfo();
        }
    }
}
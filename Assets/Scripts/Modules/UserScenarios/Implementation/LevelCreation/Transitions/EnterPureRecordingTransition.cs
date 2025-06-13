using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.CharacterManagement;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;
using Navigation.Args;
using UIManaging.Localization;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    /// <summary>
    /// Entering Level Editor with the only ability to record new event, but not preview/delete previous
    /// </summary>
    [UsedImplicitly]
    internal sealed class EnterPureRecordingTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ITemplateProvider _templateProvider;
        private readonly CharacterManager _characterManager;
        
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;

        public EnterPureRecordingTransition(ITemplateProvider templateProvider, CharacterManager characterManager)
        {
            _templateProvider = templateProvider;
            _characterManager = characterManager;
        }

        public override ScenarioState To => ScenarioState.LevelEditor;

        private long? CreateEventTemplateId => Context.PostRecordEditor.PostRecordEditorSettings.EventCreationSettings
                                                      .TemplateId;
        private bool IsCreationBasedOnTemplate => CreateEventTemplateId.HasValue;
        
        protected override async Task UpdateContext()
        {
            Context.LevelEditor.ShowLoadingPagePopup = false;
            Context.NavigationMessage = _loadingOverlayLocalization.GoingBackToRecordHeader;
            Context.LevelEditor.NewEventsDeletionOnly = true;
            Context.LevelEditor.DraftEventData = default;
            Context.LevelEditor.OnMoveBack = ScenarioState.PostRecordEditor;
            Context.LevelEditor.ExitButtonBehaviour = ExitButtonBehaviour.DiscardingAllRecordMenu;

            Context.LevelEditor.CharactersToUseInTemplate = IsCreationBasedOnTemplate ? await SelectCharactersForTemplate() : null;
        }

        /// <summary>
        /// Selects characters from existed level and decides which one should replace the character from template
        /// The main user character is always included and replaces the target character in the template
        /// (or the first one if the template is a group focused)
        /// </summary>
        private async Task<Dictionary<long, CharacterFullInfo>> SelectCharactersForTemplate()
        {
            var output = new Dictionary<long, CharacterFullInfo>();
            
            var templateEvent = await _templateProvider.GetTemplateEvent(CreateEventTemplateId.Value);
            var mainCharacter = await _characterManager.GetSelectedCharacterFullInfo();

            var needCharacterCount = templateEvent.CharactersCount();
            if (needCharacterCount == 1)
            {
                var originId = templateEvent.GetTargetCharacterController().CharacterId;
                output[originId] = mainCharacter;
                return output;
            }

            var candidates = GetCandidatesForReplacing().Where(x=>x.Id != mainCharacter.Id).ToArray();
            
            var characterControllers =  templateEvent.CharacterController;

            var candidateIndex = 0;
            for (var i = 0; i < characterControllers.Count; i++)
            {
                var controller = characterControllers.ElementAt(i);
                
                var isFocused = templateEvent.TargetCharacterSequenceNumber == controller.ControllerSequenceNumber || 
                                templateEvent.IsGroupFocus() && controller.ControllerSequenceNumber == 0;
                var characterToReplace = isFocused
                    ? mainCharacter 
                    : candidates[candidateIndex++];
                output[controller.CharacterId] = characterToReplace;
            }

            return output;
        }

        private IEnumerable<CharacterFullInfo> GetCandidatesForReplacing()
        {
            return Context.LevelData.GetOrderedEvents().Reverse()
                          .SelectMany(x => x.CharacterController)
                          .Select(x => x.Character)
                          .DistinctBy(x => x.Id);
        }
    }
}
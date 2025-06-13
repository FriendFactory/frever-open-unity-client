using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.EditorsCommon;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups;
using UIManaging.Localization;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class CreateNewLevelBasedOnTemplateSocialActionScenario : LevelCreationScenarioBase<CreateNewLevelBasedOnTemplateScenarioArgs>, ICreateNewLevelBasedOnTemplateScenario
    {
        public CreateNewLevelBasedOnTemplateSocialActionScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider)
            : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(Resolve<ITemplateProvider>(), InitialArgs, Resolve<CharacterManager>(),
                                       Resolve<IUndressingCharacterService>(), Resolve<IMetadataProvider>(),
                                       InitialArgs.ShowGridPage, Resolve<TemplateCharacterSelectionLocalization>());
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetup(LevelCreationSetup.DefaultLevelCreation);
            return Task.FromResult(setup.States);
        }

        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly CreateNewLevelBasedOnTemplateScenarioArgs _args;
            private readonly ITemplateProvider _templateProvider;
            private readonly IUndressingCharacterService _undressingCharacterService;
            private readonly CharacterManager _characterManager;
            private readonly bool _showTemplateGrid;
            private readonly IMetadataProvider _metadataProvider;
            private readonly TemplateCharacterSelectionLocalization _characterSelectionLocalization;

            private MetadataStartPack MetadataStartPack => _metadataProvider.MetadataStartPack;
            
            private ScenarioState _to;
            public override ScenarioState To => _to;

            public EntryTransition(
                ITemplateProvider templateProvider,
                CreateNewLevelBasedOnTemplateScenarioArgs args,
                CharacterManager characterManager,
                IUndressingCharacterService undressingCharacterService,
                IMetadataProvider metadataProvider,
                bool showGridPage, TemplateCharacterSelectionLocalization characterSelectionLocalization) : base(args)
            {
                _templateProvider = templateProvider;
                _args = args;
                _characterManager = characterManager;
                _showTemplateGrid = showGridPage;
                _characterSelectionLocalization = characterSelectionLocalization;
                _undressingCharacterService = undressingCharacterService;
                _metadataProvider = metadataProvider;
            }

            protected override async Task UpdateContext()
            {
                await base.UpdateContext();
                Context.SocialActionId = _args.SocialActionId;
                Context.RecommendationId = _args.RecommendationId;
                Context.InitialTemplateId = _args.Template.Id;
                Context.LevelEditor.TemplateId = Context.InitialTemplateId;
                Context.LevelEditor.LinkTemplateToEvent = true;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                Context.LevelEditor.OnLevelEditorLoaded = _args.OnTemplateLoaded;
                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                Context.LevelEditor.ShowDressingStep = true;
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;
                var templateEvent = await _templateProvider.GetTemplateEvent(Context.InitialTemplateId.Value);
                if (templateEvent == null)
                {
                    throw new Exception($"Failed to download template: {Context.InitialTemplateId}");
                }

                var charactersToReplace = templateEvent.GetUniqueCharacterIds();
                Context.CharacterSelection.CharacterToReplaceIds = charactersToReplace;
                Context.CharacterSelection.Race = MetadataStartPack.GetRaceByUniverseId(_args.Universe.Id);
                var characterCount = charactersToReplace.Length;
                var showCharacterSelection = characterCount > 1;
                if (_showTemplateGrid)
                {
                    _to = ScenarioState.TemplateGrid;
                    await base.UpdateContext();
                    return;
                }

                if (showCharacterSelection)
                {
                    Context.CharacterSelection.Header = _characterSelectionLocalization.Header;
                    Context.CharacterSelection.HeaderDescription = string.Empty;
                    Context.CharacterSelection.ReasonText = string.Format(_characterSelectionLocalization.Reason, characterCount);
                    _to = ScenarioState.CharacterSelection;
                    Context.OnLevelCreationCanceled += _args.OnCancelCallback;
                }
                else
                {
                    Context.LevelEditor.ShowLoadingPagePopup = true;
                    _to = ScenarioState.LevelEditor;
                    await SetMainCharacterAsPicked(templateEvent);
                    Context.LevelEditor.CharactersToUseInTemplate = Context.CharacterSelection.PickedCharacters;
                }

                await base.UpdateContext();
            }

            private async Task SetMainCharacterAsPicked(Event templateEvent)
            {
                Context.CharacterSelection.PickedCharacters = new Dictionary<long, CharacterFullInfo>();
                var replaceTarget = templateEvent.GetUniqueCharacterIds().First();
                var characterId = _characterManager.RaceMainCharacters[Context.CharacterSelection.Race.Id];
                Context.CharacterSelection.PickedCharacters[replaceTarget] =
                    await _characterManager.GetCharacterAsync(characterId);
            }
        }
    }
}
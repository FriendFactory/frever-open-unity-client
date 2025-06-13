using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common.Publishers;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.EditorsCommon;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class CreateNewLevelScenario : LevelCreationScenarioBase<CreateNewLevelScenarioArgs>, ICreateNewLevelScenario
    {
        private readonly IDataFetcher _dataFetcher;

        public CreateNewLevelScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider, IDataFetcher dataFetcher) : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
            _dataFetcher = dataFetcher;
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, _dataFetcher, Resolve<CharacterManager>(), Resolve<ITemplateProvider>(), Resolve<IUndressingCharacterService>());
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetup(LevelCreationSetup.DefaultLevelCreation);
            return Task.FromResult(setup.States);
        }
        
        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly CreateNewLevelScenarioArgs _args;
            private readonly IDataFetcher _dataFetcher;
            private readonly CharacterManager _characterManager;
            private readonly ITemplateProvider _templateProvider;
            private readonly IUndressingCharacterService _undressingCharacterService;

            private MetadataStartPack MetadataStartPack => _dataFetcher.MetadataStartPack;

            public EntryTransition(CreateNewLevelScenarioArgs args, IDataFetcher dataFetcher, CharacterManager characterManager, ITemplateProvider templateProvider, IUndressingCharacterService undressingCharacterService) : base(args)
            {
                _args = args;
                _dataFetcher = dataFetcher;
                _characterManager = characterManager;
                _templateProvider = templateProvider;
                _undressingCharacterService = undressingCharacterService;
            }

            public override ScenarioState To => ScenarioState.LevelEditor;
        
            protected override async Task UpdateContext()
            {
                Context.Hashtag = _args.Hashtag;
                Context.Music = _args.Music;
                Context.InitialTemplateId = _dataFetcher.DefaultUserAssets.TemplateId;
                Context.OpenedFromChat = _args.OpenedFromChat;
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;
                if (Context.OpenedFromChat != null)
                {
                    Context.PublishContext.PublishingType = PublishingType.VideoMessage;
                }
                Context.InitialTemplateId = _dataFetcher.DefaultTemplateId;
                Context.LevelEditor.TemplateId = Context.InitialTemplateId;
                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                Context.LevelEditor.ShowLoadingPagePopup = true;
                Context.LevelEditor.ShowTemplateCreationStep = _args.ShowTemplateCreationStep;
                Context.LevelEditor.ShowDressingStep = _args.ShowDressingStep;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                
                //todo: remove temp code(after integration Universe popup):
                if (_args.Universe == null)
                {
                    var gender = _characterManager.SelectedCharacter.GenderId;
                    _args.Universe = MetadataStartPack.GetUniverseByGenderId(gender);
                }
                
                Context.CharacterSelection.Race = MetadataStartPack.GetRaceByUniverseId(_args.Universe.Id);
                await SetMainCharacterAsPicked();
                Context.LevelEditor.CharactersToUseInTemplate = Context.CharacterSelection.PickedCharacters;
                await base.UpdateContext();
            }

            private async Task SetMainCharacterAsPicked()
            {
                var templateEvent = await _templateProvider.GetTemplateEvent(Context.InitialTemplateId.Value);
                var replaceTarget = templateEvent.GetUniqueCharacterIds().First();
                
                Context.CharacterSelection.PickedCharacters = new Dictionary<long, CharacterFullInfo>();
                var characterId = _characterManager.RaceMainCharacters[Context.CharacterSelection.Race.Id];
                Context.CharacterSelection.PickedCharacters[replaceTarget] = await _characterManager.GetCharacterAsync(characterId);
            }
        }
    }
}
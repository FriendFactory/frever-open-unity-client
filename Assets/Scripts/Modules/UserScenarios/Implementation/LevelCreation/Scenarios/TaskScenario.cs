using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Common.ModelsMapping;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.EditorsCommon;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Navigation.Core;
using UIManaging.SnackBarSystem;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class TaskScenario : LevelCreationScenarioBase<TaskScenarioArgs>, ITaskScenario
    {
        public TaskScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider) : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
           var entry = Resolve<EntryTransition>();
           entry.SetArgs(InitialArgs);
           return entry;
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetupForTask(InitialArgs.TaskInfo);
            return Task.FromResult(setup.States);
        }

        protected override Task<EditorsSettings> GetSettings()
        {
            return EditorSettingsProvider.GetEditorSettingsForTask(InitialArgs.TaskInfo.Id);
        }

        [UsedImplicitly]
        private sealed class EntryTransition: TaskStartTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly PageManager _pageManager;
            
            private TaskScenarioArgs _args;
            private ScenarioState _startScenarioState;
            private IMetadataProvider _metadataProvider;

            public override ScenarioState To => _startScenarioState;
            
            private MetadataStartPack MetadataStartPack => _metadataProvider.MetadataStartPack;

            public EntryTransition(PageManager pageManager, IBridge bridge, IMapper mapper, SnackBarHelper snackBarHelper, 
                ILevelHelper levelHelper, ITemplateProvider templateProvider, CharacterManager characterManager, IMetadataProvider metadataProvider): 
                base(bridge, mapper, snackBarHelper, levelHelper, templateProvider, characterManager, metadataProvider)
            {
                _pageManager = pageManager;
                _metadataProvider = metadataProvider;
            }

            public void SetArgs(TaskScenarioArgs args)
            {
                _args = args;
            }

            protected override async Task UpdateContext()
            {
                var taskInfo = _args.TaskInfo;

                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                Context.PostRecordEditor.CheckIfUserMadeEnoughChangesForTask = true;
                Context.CharacterSelection.Race = MetadataStartPack.GetRaceByUniverseId(_args.Universe.Id);
                
                await InitializeTask(taskInfo);
                await SetupInitialState(taskInfo, "use in challenge");
                await InitializeCharacter(taskInfo.IsDressed);
                
                InitializeShowingTaskInfo(taskInfo.Pages.First());
                Context.OpenedFromPage = _pageManager.CurrentPage.Id;
            }
            
            protected override void SetupSkippingCharacterSelection(TaskFullInfo taskInfo)
            {
                _startScenarioState = taskInfo.Pages[0].ToScenarioState();
            }

            protected override void SetupCharacterSelection()
            {
                _startScenarioState = ScenarioState.CharacterSelection;
            }
        }
    }
}
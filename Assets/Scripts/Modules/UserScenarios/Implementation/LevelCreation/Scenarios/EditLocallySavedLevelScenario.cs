using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.EditorsCommon;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class EditLocallySavedLevelScenario: LevelCreationScenarioBase<EditLocalSavedLevelScenarioArgs>, IEditLocallySavedLevelScenario
    {
        public EditLocallySavedLevelScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider) 
            : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, ScenarioState.LevelEditor);
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetup(LevelCreationSetup.EditLocallySavedLevel);
            return Task.FromResult(setup.States);
        }

        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly EditLocalSavedLevelScenarioArgs _args;
            
            public override ScenarioState To { get; }

            public EntryTransition(EditLocalSavedLevelScenarioArgs args, ScenarioState to) : base(args)
            {
                _args = args;
                To = to;
            }

            protected override async Task UpdateContext()
            {
                Context.LevelData = _args.LocallySavedLevel;
                Context.OriginalLevelData = _args.OriginalFromServer;
                Context.LevelToStartOver =  await _args.LocallySavedLevel.CloneAsync();
                Context.LevelEditor.OnMoveBack = ScenarioState.FeedExit;
                Context.LevelEditor.ShowLoadingPagePopup = true;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;
                await base.UpdateContext();
            }
        }
    }
}
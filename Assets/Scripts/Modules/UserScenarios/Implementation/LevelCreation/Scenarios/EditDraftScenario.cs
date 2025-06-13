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
    internal sealed class EditDraftScenario: LevelCreationScenarioBase<EditDraftScenarioArgs>, IEditDraftScenario
    {
        public EditDraftScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider) 
            : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs);
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetup(LevelCreationSetup.DefaultLevelCreation);
            return Task.FromResult(setup.States);
        }

        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly EditDraftScenarioArgs _args;

            public override ScenarioState To => ScenarioState.PostRecordEditor;

            public EntryTransition(EditDraftScenarioArgs args) : base(args)
            {
                _args = args;
            }

            protected override async Task UpdateContext()
            {
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;
                Context.LevelData = _args.Draft;
                Context.OriginalLevelData = await _args.Draft.CloneAsync();
                Context.LevelToStartOver = await _args.Draft.CloneAsync();
                Context.PostRecordEditor.ShowPageLoadingPopup = true;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber = 1;
                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                await base.UpdateContext();
            }
        }
    }
}
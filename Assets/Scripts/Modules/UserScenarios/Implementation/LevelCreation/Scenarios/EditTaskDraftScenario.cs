using System;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.Tasks;
using Common.Services;
using Extensions;
using JetBrains.Annotations;
using Modules.EditorsCommon;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class EditTaskDraftScenario: LevelCreationScenarioBase<EditTaskDraftScenarioArgs>, IEditTaskDraftScenario
    {
        private readonly IUserTaskProvider _userTaskProvider;
        private TaskFullInfo _taskFullInfo;

        public EditTaskDraftScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider, IUserTaskProvider userTaskProvider) : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
            _userTaskProvider = userTaskProvider;
        }

        protected override async Task BeforeSetup()
        {
            var taskResponse = await _userTaskProvider.GetTaskFullInfoAsync(InitialArgs.TaskId);
            if (taskResponse.IsError)
            {
                throw new Exception($"Failed to load task info. {taskResponse.ErrorMessage}");
            }

            _taskFullInfo = taskResponse.Model;
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs,ScenarioState.PostRecordEditor, _taskFullInfo);
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetupForTask(_taskFullInfo);
            return Task.FromResult(setup.States);
        }

        protected override Task<EditorsSettings> GetSettings()
        {
            return EditorSettingsProvider.GetEditorSettingsForTask(InitialArgs.TaskId);
        }

        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly EditTaskDraftScenarioArgs _args;
            private readonly TaskFullInfo _taskFullInfo;
            
            public override ScenarioState To { get; }

            public EntryTransition(EditTaskDraftScenarioArgs args, ScenarioState to, TaskFullInfo taskFullInfo) : base(args)
            {
                _args = args;
                To = to;
                _taskFullInfo = taskFullInfo;
            }

            protected override async Task UpdateContext()
            {
                Context.LevelData = _args.Draft;
                Context.Task = _taskFullInfo;
                Context.PostRecordEditor.ShowPageLoadingPopup = true;
                Context.PostRecordEditor.CheckIfUserMadeEnoughChangesForTask = true;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                Context.OriginalLevelData = await _args.Draft.CloneAsync();
                Context.LevelToStartOver = await _args.Draft.CloneAsync();
                Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber = 1;
                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;
                await base.UpdateContext();
            }
        }
    }
}
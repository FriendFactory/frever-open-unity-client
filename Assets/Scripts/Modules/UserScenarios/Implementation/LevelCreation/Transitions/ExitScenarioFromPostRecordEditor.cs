using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class ExitScenarioFromPostRecordEditor: TransitionBase<ILevelCreationScenarioContext>
    {
        private readonly ILevelManager _levelManager;
        private readonly PopupManager _popupManager;

        private readonly ScenarioState _destinationState;

        public ExitScenarioFromPostRecordEditor(ScenarioState scenarioState, ILevelManager levelManager, PopupManager popupManager)
        {
            _destinationState = scenarioState;
            _levelManager = levelManager;
            _popupManager = popupManager;
        }

        public override ScenarioState To => _destinationState;
        
        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            _levelManager.StopCurrentPlayMode();
            _levelManager.CancelLoading();
            _levelManager.ClearTempFiles();
            _popupManager.ClosePopupByType(PopupType.SimulatedPageLoading);
            await base.OnRunning();
        }
    }
}
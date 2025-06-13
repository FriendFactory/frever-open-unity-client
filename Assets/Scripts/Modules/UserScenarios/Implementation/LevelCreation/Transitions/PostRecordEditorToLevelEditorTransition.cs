using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using UIManaging.Localization;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class PostRecordEditorToLevelEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        
        public override ScenarioState To => ScenarioState.LevelEditor;

        public PostRecordEditorToLevelEditorTransition(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        protected override Task UpdateContext()
        {
            Context.NavigationMessage = _loadingOverlayLocalization.GoingBackToRecordHeader;
            Context.LevelEditor.TemplateId = null;
            Context.LevelEditor.OpenVideoUploadMenu = false;
            Context.LevelEditor.NewEventsDeletionOnly = false;
            Context.LevelEditor.ExitButtonBehaviour = ExitButtonBehaviour.StartOverMenu;
            Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
            Context.LevelEditor.ShowLoadingPagePopup = false;
            
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            var lastEvent = Context.LevelData.GetLastEvent();
            _levelManager.UnloadNotUsedByEventsAssets(lastEvent);
            return base.OnRunning();
        }
    }
}
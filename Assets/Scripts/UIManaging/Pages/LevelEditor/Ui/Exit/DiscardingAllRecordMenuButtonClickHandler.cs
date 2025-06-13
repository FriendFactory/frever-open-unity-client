using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Navigation.Args;
using UIManaging.PopupSystem;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Exit
{
    [UsedImplicitly]
    internal sealed class DiscardingAllRecordMenuButtonClickHandler : ExitButtonClickHandler
    {
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private LevelEditorPageModel _levelEditorPageModel;
        [Inject] private ILevelManager _levelManager;
        
        public override ExitButtonBehaviour ExitButtonBehaviour => ExitButtonBehaviour.DiscardingAllRecordMenu;
        public override Task HandleClickAsync()
        {
            if (ShouldDisplayMenuPopup())
            {
                _popupManagerHelper.OpenDiscardAllRecordingEventsPopup(OnDiscardRecordings, null);
            }
            else
            {
                _levelEditorPageModel.RequestExit(false);
            }
            return Task.CompletedTask;
        }

        private bool ShouldDisplayMenuPopup()
        {
            var hasRecordings = _levelManager.CurrentLevel.Event.Count >
                                _levelEditorPageModel.OpeningPageArgs.LevelData.Event.Count;
            return hasRecordings;
        }

        private void OnDiscardRecordings()
        {
            _levelManager.CurrentLevel = _levelEditorPageModel.OriginalEditorLevel;
            _levelEditorPageModel.RequestExit(false);
        }
    }
}
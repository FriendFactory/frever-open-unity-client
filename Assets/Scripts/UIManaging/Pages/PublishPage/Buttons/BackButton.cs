using Modules.LevelManaging.Editing.LevelManagement;
using Zenject;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Localization;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class BackButton : PublishPageButtonBase
    {
        [Inject] private PageManager _pageManager;
        [Inject] private ILevelManager _levelManager;
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        
        protected override void OnButtonClicked()
        {
            var editorArgs = new PostRecordEditorArgs
            {
                NavigationMessage = _loadingOverlayLocalization.GoingBackToClipEditorHeader,
                LevelData = _levelManager.CurrentLevel
            };

            _pageManager.MoveBackTo(PageId.PostRecordEditor, editorArgs);
        }
    }
}

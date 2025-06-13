using Extensions;
using Models;
using Navigation.Args;
using Navigation.Core;

namespace UIManaging.Common.Buttons
{
    public sealed class LevelPreviewButton: LevelLoadButton
    {
        protected override void LoadLevel(Level level)
        {
            var editorArgs = new PostRecordEditorArgs()
            {
                LevelData = level,
                IsPreviewMode = true,
                NavigationMessage = "Proceeding to the Editor...",
                OnPreviewCompleted = OnPreviewComplete
            };

            UiManager.MoveNext(PageId.PostRecordEditor, editorArgs);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------\

        private void OnPreviewComplete()
        {
            UiManager.MoveBack();
        }
    }
}
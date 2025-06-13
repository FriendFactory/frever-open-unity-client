using Common.Abstract;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Recording
{
    internal sealed class RecordingStepProgressPanel: BaseContextlessPanel
    {
        [SerializeField] private OpenPostRecordEditorButton _nextButton;
        
        protected override void OnInitialized()
        {
            _nextButton.Initialize();
        }
        
        protected override void BeforeCleanUp()
        {
            _nextButton.CleanUp();
        }
    }
}
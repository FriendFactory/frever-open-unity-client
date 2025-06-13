using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals
{
    internal sealed class AssetSelectionButtonClickedSignal : BaseSelectionButtonClickedSignal<AssetSelectionProgressStepType>
    {
        public AssetSelectionButtonClickedSignal(AssetSelectionProgressStepType stepType) : base(stepType)
        {
        }
    }
}
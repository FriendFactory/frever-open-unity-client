using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals
{
    [RequireComponent(typeof(Button))]
    internal sealed class AssetSelectionButtonClickedSignalEmitter: BaseSelectionButtonClickedSignalEmitter<AssetSelectionProgressStepType, AssetSelectionButtonClickedSignal>
    {
        protected override AssetSelectionButtonClickedSignal GetSignal(AssetSelectionProgressStepType stepType)
        {
            return new AssetSelectionButtonClickedSignal(stepType);
        }
    }
}
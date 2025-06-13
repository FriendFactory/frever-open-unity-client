using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal sealed class AssetSelectionButtonClickedAmplitudeEvent: BaseSelectionButtonClickedAmplitudeEvent<AssetSelectionProgressStepType>
    {
        public AssetSelectionButtonClickedAmplitudeEvent(AssetSelectionProgressStepType stepType) : base(stepType) { }

        protected override string GetName(AssetSelectionProgressStepType stepType)
        {
            return stepType.GetAssetSelectionButtonClickedEventName();
        }
    }
}
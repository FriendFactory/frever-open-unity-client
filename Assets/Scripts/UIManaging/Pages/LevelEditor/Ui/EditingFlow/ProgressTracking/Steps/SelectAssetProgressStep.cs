namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps
{
    internal class SelectAssetProgressStep: BaseProgressStep<AssetSelectionProgressStepType>
    {
        public SelectAssetProgressStep(AssetSelectionProgressStepType progressType, bool isCompleted = false) : base(progressType)
        {
            IsCompleted = isCompleted;
        }

        public override void Validate(AssetSelectionProgressStepType selectionStepType)
        {
            IsCompleted = StepType == selectionStepType;
        }
    }
}
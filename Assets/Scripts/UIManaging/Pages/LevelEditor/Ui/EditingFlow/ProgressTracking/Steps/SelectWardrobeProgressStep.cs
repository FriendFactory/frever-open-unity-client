namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps
{
    internal sealed class SelectWardrobeProgressStep : BaseProgressStep<WardrobeSelectionProgressStepType>
    {
        public WardrobeSelectionCategoryData CategoryData { get; }

        private readonly bool _initialState;
        
        public SelectWardrobeProgressStep(WardrobeSelectionCategoryData categoryData, bool isCompleted = false) : base(categoryData.ProgressStepType)
        {
            _initialState = isCompleted;
            
            IsCompleted = isCompleted;
            CategoryData = categoryData;
        }

        public override void Validate(WardrobeSelectionProgressStepType selectionStepType)
        {
            IsCompleted = StepType == selectionStepType;
        }

        public void Complete()
        {
            IsCompleted = true;
        }
        
        public override void Reset()
        {
            IsCompleted = _initialState;
        }
    }
}
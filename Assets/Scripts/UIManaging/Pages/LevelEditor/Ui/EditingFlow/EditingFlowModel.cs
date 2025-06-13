namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class EditingFlowModel
    {
        public LevelEditorState[] Steps { get; }
        public LevelEditorState InitialState { get; }
        
        public EditingFlowModel(LevelEditorState[] steps, LevelEditorState initialState)
        {
            Steps = steps;
            InitialState = initialState;
        }
    }
}
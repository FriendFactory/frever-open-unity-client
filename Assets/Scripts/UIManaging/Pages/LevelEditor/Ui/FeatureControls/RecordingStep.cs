namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal sealed class RecordingStep : EditingStep
    {
        public override int OrderIndex => 2;
        protected override void OnRun()
        {
            LevelEditorPageModel.ChangeState(LevelEditorState.Default);
        }
    }
}
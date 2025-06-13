namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal sealed class TemplateSetupStep: EditingStep
    {
        public override int OrderIndex => 0;
        
        protected override void OnRun()
        {
            LevelEditorPageModel.ChangeState(LevelEditorState.TemplateSetup);
        }
    }
}
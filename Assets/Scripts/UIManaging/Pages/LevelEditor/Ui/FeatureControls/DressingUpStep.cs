using Modules.LevelManaging.Editing.LevelManagement;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal sealed class DressingUpStep : EditingStep
    {
        [Inject] private ILevelManager _levelManager;
        public override int OrderIndex => 1;
        protected override void OnRun()
        {
            LevelEditorPageModel.ChangeState(LevelEditorState.Dressing);
            _levelManager.EditingCharacterSequenceNumber = 0;
        }
    }
}
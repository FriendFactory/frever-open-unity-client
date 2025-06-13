using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Common;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls.ViewControls
{
    internal sealed class VoiceFilterButtonStateControl : DynamicSettingsStateControl<IVoiceFilterFeatureControl, LevelEditorState>
    {
        [Inject] private ILevelManager _levelManager;
        
        protected override bool ShouldBeActive(LevelEditorState nextState)
        {
            return base.ShouldBeActive(nextState) && !_levelManager.TargetEvent.HasMusic();
        }
    }
}
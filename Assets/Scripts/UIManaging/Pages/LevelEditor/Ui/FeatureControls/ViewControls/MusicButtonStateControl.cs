using System.Linq;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Common;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls.ViewControls
{
    internal sealed class MusicButtonStateControl : DynamicSettingsStateControl<IMusicSelectionFeatureControl, LevelEditorState>
    {
        [Inject] private ILevelManager _levelManager;

        protected override bool ShouldBeActive(LevelEditorState nextState)
        {
            if(!FeatureControl.IsFeatureEnabled || !ShouldBeActiveOnStates.Contains(nextState)) return false;
            if (nextState == LevelEditorState.Preview)
            {
                return _levelManager.CurrentLevel.Event.Any(x => x.HasMusic());
            }
            
            return base.ShouldBeActive(nextState);
        }
    }
}
using System.Linq;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Common;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls.ViewControls
{
    internal sealed class MusicButtonStateControl : DynamicSettingsStateControl<IMusicSelectionFeatureControl, PostRecordEditorState>
    {
        [Inject] private ILevelManager _levelManager;

        protected override bool ShouldBeActive(PostRecordEditorState nextState)
        {
            if(!FeatureControl.IsFeatureEnabled || !ShouldBeActiveOnStates.Contains(nextState)) return false;
            if (nextState == PostRecordEditorState.Preview)
            {
                return _levelManager.CurrentLevel.Event.Any(x => x.HasMusic());
            }
            
            return base.ShouldBeActive(nextState);
        }
    }
}
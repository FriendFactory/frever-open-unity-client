using System.Linq;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Common;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls.ViewControls
{
    internal sealed class DeleteEventButtonStateControl : DynamicSettingsStateControl<IDeleteEventFeatureControl, PostRecordEditorState>
    {
        [Inject] private ILevelManager _levelManager;

        protected override bool ShouldBeActive(PostRecordEditorState nextState)
        {
            return ShouldBeActiveOnStates.Contains(nextState) && FeatureControl.CanDeleteEvent(_levelManager.TargetEvent);
        }
    }
}
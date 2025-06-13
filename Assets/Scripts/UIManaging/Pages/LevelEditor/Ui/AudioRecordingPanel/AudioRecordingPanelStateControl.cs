using System.Linq;
using Modules.LevelManaging.Editing;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class AudioRecordingPanelStateControl: DynamicSettingsStateControl<IMusicSelectionFeatureControl, LevelEditorState>
    {
        [Inject] private AudioRecordingStateController _audioRecordingStateController;

        protected override bool ShouldBeActive(LevelEditorState nextState)
        {
            if(!FeatureControl.IsFeatureEnabled || !ShouldBeActiveOnStates.Contains(nextState)) return false;

            var musicSelected = _audioRecordingStateController.State == AudioRecordingState.MusicSelected || _audioRecordingStateController.State == AudioRecordingState.MusicPreviewed;
            
            if (nextState == LevelEditorState.Preview || nextState == LevelEditorState.Recording)
            {
                return musicSelected;
            }
            
            return base.ShouldBeActive(nextState);
        }
        
    }
}
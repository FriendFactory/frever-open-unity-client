using Modules.Amplitude;
using Modules.Amplitude.Events.Core;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal sealed class EditingFlowStepChangeAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => AmplitudeEventConstants.EventNames.PAGE_CHANGE;
        
        public EditingFlowStepChangeAmplitudeEvent(LevelEditorState state, EditingFlowType flowType)
        {
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.PAGE_NAME, GetPageName(state));
            _eventProperties.Add("Flow Type", flowType.ToString());
        }

        private string GetPageName(LevelEditorState state)
        {
            return state switch
            {
                LevelEditorState.TemplateSetup => "CreateChallenge",
                LevelEditorState.Dressing => "DressUpPreview",
                // wardrobe panel doesn't have a separated state
                LevelEditorState.PurchasableAssetSelection => "DressUpWardrobe",
                LevelEditorState.Default => "Recording",
                _ => "Unknown"
            };
        }
    }
}
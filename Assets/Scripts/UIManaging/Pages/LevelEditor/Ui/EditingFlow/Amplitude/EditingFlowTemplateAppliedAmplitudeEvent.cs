using System.Linq;
using Modules.Amplitude;
using Modules.Amplitude.Events.Core;
using Modules.LevelManaging.Editing;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal sealed class EditingFlowTemplateAppliedAmplitudeEvent : BaseAmplitudeEvent
    {
        public override string Name => AmplitudeEventConstants.EventNames.TEMPLATE_USED;

        public EditingFlowTemplateAppliedAmplitudeEvent(ApplyingTemplateArgs args)
        {
            var characterIds = args.ReplaceCharactersData.Select(x => x.ReplaceByCharacter.Id).ToArray();
            
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.TEMPLATE_ID, args.Template.Id);
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.CHARACTER_IDS, characterIds);
        }
    }
}
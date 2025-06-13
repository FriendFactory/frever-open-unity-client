using Modules.Amplitude.Events.Core;
using Navigation.Args;
using Navigation.Core;

namespace Modules.Amplitude.Events.PageChange
{
    internal sealed class UmaEditorPageChangeAmplitudeEventDecorator: AmplitudeEventDecorator
    {
        public UmaEditorPageChangeAmplitudeEventDecorator(IDecoratableAmplitudeEvent wrappedEvent, PageArgs pageArgs) : base(wrappedEvent)
        {
            if (pageArgs is not UmaEditorArgs umaEditorArgs) return;
            
            var characterId = umaEditorArgs.IsNewCharacter ? null : umaEditorArgs.Character?.Id;
            var genderId = umaEditorArgs.IsNewCharacter ? umaEditorArgs.Gender?.Id : umaEditorArgs.Character?.GenderId;
            
            _wrappedEvent.AddProperty(AmplitudeEventConstants.EventProperties.CHARACTER_ID, characterId);
            _wrappedEvent.AddProperty(AmplitudeEventConstants.EventProperties.GENDER_ID, genderId);
        }
    }
}
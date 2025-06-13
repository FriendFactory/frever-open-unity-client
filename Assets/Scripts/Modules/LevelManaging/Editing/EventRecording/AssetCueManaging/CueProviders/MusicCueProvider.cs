using Extensions;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders
{
    internal sealed class MusicCueProvider: CueProviderBase
    {
        private readonly ITemplateProvider _templateProvider;

        public MusicCueProvider(IContext context, IEventTemplateManager eventTemplateManager, ITemplateProvider templateProvider) 
            : base(context, eventTemplateManager)
        {
            _templateProvider = templateProvider;
        }

        public int GetActivationCue(Event targetEvent)
        {
            var previousEvent = GetPreviousEvent();
            
            if (ShouldUseCueFromTemplate(targetEvent, previousEvent))
            {
                return GetActivationCueFromTemplate(targetEvent.TemplateId.Value);
            }
            return GetRegularActivationCue(targetEvent, previousEvent);
        }
        
        public int GetEndCue(Event recorded)
        {
            var currentMusicController = recorded.GetMusicController();
            return recorded.Length + currentMusicController.ActivationCue;
        }

        private bool ShouldUseCueFromTemplate(Event recordingEvent, Event previousEvent)
        {
            if (!recordingEvent.TemplateId.HasValue) return false;
            
            var eventsHasSameTemplates = recordingEvent.IsUsingTheSameTemplate(previousEvent);
            var eventsHasTheSameAudio = recordingEvent.HasTheSameMusic(previousEvent);

            if (eventsHasSameTemplates && eventsHasTheSameAudio)
            {
                return IsMadeFromTemplate(recordingEvent);
            }

            var template = _templateProvider.GetTemplateEventFromCache(recordingEvent.TemplateId.Value);
            var targetHasTheSameAudioAsTemplate = recordingEvent.HasTheSameMusic(template);
            return targetHasTheSameAudioAsTemplate;
        }

        private int GetActivationCueFromTemplate(long templateId)
        {
            var template = _templateProvider.GetTemplateEventFromCache(templateId);
            return template.GetMusicController().ActivationCue;
        }
        
        private int GetRegularActivationCue(Event targetEvent, Event previousEvent)
        {
            if (targetEvent.HasTheSameMusic(previousEvent))
            {
                var lastEventController = previousEvent.GetMusicController();
                return lastEventController.EndCue;
            }

            var currentMusicController = targetEvent.GetMusicController();
            return currentMusicController.ActivationCue;
        }
    }
}
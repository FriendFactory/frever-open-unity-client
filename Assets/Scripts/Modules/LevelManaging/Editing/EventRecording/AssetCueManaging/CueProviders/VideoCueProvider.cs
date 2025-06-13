using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders
{
    internal sealed class VideoCueProvider: CueProviderBase
    {
        private readonly IAssetManager _assetManager;
        private readonly ITemplateProvider _templateProvider;

        public VideoCueProvider(IContext context, IEventTemplateManager eventTemplateManager, IAssetManager assetManager, ITemplateProvider templateProvider) 
            : base(context, eventTemplateManager)
        {
            _assetManager = assetManager;
            _templateProvider = templateProvider;
        }

        public int? GetActivationCue(Event recordingEvent)
        {
            if (recordingEvent.GetVideo() == null) return null;

            var previousEvent = GetPreviousEvent();
            if (ShouldUseCueFromTemplate(recordingEvent, previousEvent))
            {
                return GetActivationCueFromTemplate(recordingEvent.TemplateId.Value);
            }
            return GetRegularActivationCue(recordingEvent, previousEvent);
        }
        
        public int? GetEndCue(Event recorded)
        {
            if (recorded.GetVideo() == null) return null;
            var currentController = recorded.GetSetLocationController();
            return currentController.VideoActivationCue.GetValueOrDefault() + recorded.Length;
        }
        
        private bool ShouldUseCueFromTemplate(Event recordingEvent, Event previousEvent)
        {
            if (!recordingEvent.TemplateId.HasValue) return false;
            
            var eventsHasSameTemplates = recordingEvent.IsUsingTheSameTemplate(previousEvent);
            var eventsHasTheSameVideo = recordingEvent.HasSameSetLocationVideo(previousEvent);

            if (eventsHasSameTemplates && eventsHasTheSameVideo)
            {
                return IsMadeFromTemplate(recordingEvent);
            }

            var template = _templateProvider.GetTemplateEventFromCache(recordingEvent.TemplateId.Value);
            var targetHasTheSameVideoAsTemplate = recordingEvent.HasSameSetLocationVideo(template);
            return targetHasTheSameVideoAsTemplate;
        }
        
        private int GetActivationCueFromTemplate(long templateId)
        {
            var template = _templateProvider.GetTemplateEventFromCache(templateId);
            return template.GetSetLocationController().VideoActivationCue.Value;
        }
        
        private int? GetRegularActivationCue(Event recordingEvent, Event previousEvent)
        {
            var setLocation = recordingEvent.GetSetLocation();
            if (!setLocation.AllowVideo) return null;

            var usingSameVideo = recordingEvent.HasSameSetLocationVideo(previousEvent);
            
            if (usingSameVideo && previousEvent != null)
            {
                var prevController = previousEvent.GetSetLocationController();
                return prevController.VideoEndCue.GetValueOrDefault();
            }
            
            var setLocationAsset = _assetManager.GetActiveAssets<ISetLocationAsset>().First(x=>x.Id == setLocation.Id);
            return (int)setLocationAsset.VideoPlaybackTime.ToMilliseconds();
        }
    }
}
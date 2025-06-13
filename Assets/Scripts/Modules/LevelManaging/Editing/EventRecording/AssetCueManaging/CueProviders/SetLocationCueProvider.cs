using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders
{
    internal sealed class SetLocationCueProvider: CueProviderBase
    {
        private readonly IAssetManager _assetManager;
        private readonly ITemplateProvider _templateProvider;

        public SetLocationCueProvider(IContext context, IEventTemplateManager eventTemplateManager, IAssetManager assetManager, ITemplateProvider templateProvider) : base(context, eventTemplateManager)
        {
            _assetManager = assetManager;
            _templateProvider = templateProvider;
        }

        public int GetActivationCue(Event recording)
        {
            var previous = GetPreviousEvent();
            if (ShouldUseCueFromTemplate(recording, previous))
            {
                return GetActivationCueFromTemplate(recording.TemplateId.Value);
            }
            return GetRegularActivationCue(recording, previous);
        }
        
        public int GetEndCue(Event recorded)
        {
            var currentController = recorded.GetSetLocationController();
            return currentController.ActivationCue + recorded.Length;
        }

        private bool ShouldUseCueFromTemplate(Event recordingEvent, Event previousEvent)
        {
            if (!recordingEvent.TemplateId.HasValue) return false;
            
            var eventsHasSameTemplates = recordingEvent.IsUsingTheSameTemplate(previousEvent);
            var eventsHasTheSameSetLocation = recordingEvent.HasSameSetLocation(previousEvent);

            if (eventsHasSameTemplates && eventsHasTheSameSetLocation)
            {
                return IsMadeFromTemplate(recordingEvent);
            }

            var template = _templateProvider.GetTemplateEventFromCache(recordingEvent.TemplateId.Value);
            var targetHasTheSameSetLocationAsTemplate = recordingEvent.HasSameSetLocation(template);
            return targetHasTheSameSetLocationAsTemplate;
        }

        private int GetActivationCueFromTemplate(long templateId)
        {
            var template = _templateProvider.GetTemplateEventFromCache(templateId);
            return template.GetSetLocationController().ActivationCue;
        }

        private int GetRegularActivationCue(Event recording, Event previous)
        {
            var setLocationAsset = _assetManager.GetActiveAssets<ISetLocationAsset>().First();

            var usingSameSetLocation = recording.HasSameSetLocation(previous);
            var previousController = previous?.GetSetLocationController();

            if (usingSameSetLocation && previousController != null)
            {
                return previousController.EndCue;
            }

            return setLocationAsset.PlaybackTimeMs;
        }
    }
}
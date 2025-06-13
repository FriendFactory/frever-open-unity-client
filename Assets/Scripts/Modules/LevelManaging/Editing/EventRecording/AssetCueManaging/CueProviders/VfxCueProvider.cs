using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders
{
    internal sealed class VfxCueProvider: CueProviderBase
    {
        private readonly IAssetManager _assetManager;
        private readonly ITemplateProvider _templateProvider;

        public VfxCueProvider(IContext context, IEventTemplateManager eventTemplateManager, IAssetManager assetManager, ITemplateProvider templateProvider) 
            : base(context, eventTemplateManager)
        {
            _assetManager = assetManager;
            _templateProvider = templateProvider;
        }

        public int GetActivationCue(Event recordingEvent)
        {
            var previousEvent = GetPreviousEvent();
            if (ShouldUseCueFromTemplate(recordingEvent, previousEvent))
            {
                return GetActivationCueFromTemplate(recordingEvent.TemplateId.Value);
            }
            return GetRegularActivationCue(recordingEvent, previousEvent);
        }
        
        public int GetEndCue(Event recordedEvent)
        {
            var currentController = recordedEvent.GetVfxController();
            return currentController.ActivationCue + recordedEvent.Length;
        }

        private bool ShouldUseCueFromTemplate(Event recordingEvent, Event previousEvent)
        {
            if (!recordingEvent.TemplateId.HasValue) return false;
            
            var eventsHasSameTemplates = recordingEvent.IsUsingTheSameTemplate(previousEvent);
            var eventsHasTheSameVfx = recordingEvent.HasTheSameVfx(previousEvent);

            if (eventsHasSameTemplates && eventsHasTheSameVfx)
            {
                return IsMadeFromTemplate(recordingEvent);
            }

            var template = _templateProvider.GetTemplateEventFromCache(recordingEvent.TemplateId.Value);
            var targetHasTheSameVfxAsTemplate = recordingEvent.HasTheSameVfx(template);
            return targetHasTheSameVfxAsTemplate;
        }

        private int GetActivationCueFromTemplate(long templateId)
        {
            var template = _templateProvider.GetTemplateEventFromCache(templateId);
            return template.GetVfxController().ActivationCue;
        }
        
        private int GetRegularActivationCue(Event recordingEvent, Event previousEvent)
        {
            var usingSameVfx = recordingEvent.HasTheSameVfx(previousEvent);
            
            if (usingSameVfx)
            {
                var previousController = previousEvent.GetVfxController();
                return previousController.EndCue;
            }
            
            var currentVfx = _assetManager.GetActiveAssets<IVfxAsset>().First();
            return currentVfx.PlaybackTime;
        }
    }
}
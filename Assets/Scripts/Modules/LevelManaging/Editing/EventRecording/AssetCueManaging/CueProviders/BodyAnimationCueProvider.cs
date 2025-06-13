using System.Linq;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders
{
    internal sealed class BodyAnimationCueProvider: CueProviderBase
    {
        private readonly IAssetManager _assetManager;
        private readonly ITemplateProvider _templatesProvider;

        public BodyAnimationCueProvider(IContext context, IEventTemplateManager eventTemplateManager, IAssetManager assetManager, ITemplateProvider templatesProvider) 
            : base(context, eventTemplateManager)
        {
            _assetManager = assetManager;
            _templatesProvider = templatesProvider;
        }

        public int GetActivationCue(Event recordingEvent, CharacterController characterController)
        {
            var previousEvent = GetPreviousEvent();
            if (ShouldUseCueFromTemplate(recordingEvent, previousEvent, characterController))
            {
                return GetActivationCueFromTemplate(recordingEvent.TemplateId.Value, characterController.ControllerSequenceNumber);
            }
            return GetRegularActivationCue(previousEvent, characterController);
        }
        
        public int GetEndCue(CharacterController controller, Event currentEvent)
        {
            var currentBodyAnimation = controller.GetBodyAnimationController();
            return currentBodyAnimation.ActivationCue + currentEvent.Length;
        }

        private bool ShouldUseCueFromTemplate(Event recordingEvent, Event previousEvent, CharacterController characterController)
        {
            if (!recordingEvent.TemplateId.HasValue) return false;
            
            var eventsHasSameTemplates = recordingEvent.IsUsingTheSameTemplate(previousEvent);
            var previousController =
                previousEvent?.GetCharacterController(characterController.ControllerSequenceNumber);
            var eventsHasSameBodyAnim = characterController.HasSameBodyAnimation(previousController);

            if (eventsHasSameTemplates && eventsHasSameBodyAnim)
            {
                return IsMadeFromTemplate(recordingEvent);
            }
            
            var template = _templatesProvider.GetTemplateEventFromCache(recordingEvent.TemplateId.Value);
            var controllerFromTemplate = template.GetCharacterController(characterController.ControllerSequenceNumber);
            var targetHasSameBodyAnimAsTemplate = controllerFromTemplate != null &&
                                                  controllerFromTemplate.HasSameBodyAnimation(characterController);
            return targetHasSameBodyAnimAsTemplate;
        }
        
        private int GetRegularActivationCue(Event previousEvent, CharacterController characterController)
        {
            var analogFromPreviousEvent =
                previousEvent?.GetCharacterController(characterController.ControllerSequenceNumber);

            var usingSameBody = characterController.HasSameBodyAnimation(analogFromPreviousEvent);
            
            var previousController =
                previousEvent?.GetCharacterController(characterController.ControllerSequenceNumber);
            if (usingSameBody && previousController != null)
            {
                return previousController.GetBodyAnimationController().EndCue;
            }

            var allCharacters = _assetManager.GetActiveAssets<ICharacterAsset>();
            var currentCharacter = allCharacters.First(x => x.Id == characterController.CharacterId);
            return (int)currentCharacter.PlaybackTime.ToMilliseconds();
        }

        private int GetActivationCueFromTemplate(long templateId, int controllerSequenceNumber)
        {
            var templateEvent = _templatesProvider.GetTemplateEventFromCache(templateId);
            var templateCharacterController =
                templateEvent.GetCharacterController(controllerSequenceNumber);
            var bodyAnimController = templateCharacterController.GetBodyAnimationController();
            return bodyAnimController.ActivationCue;
        }
    }
}
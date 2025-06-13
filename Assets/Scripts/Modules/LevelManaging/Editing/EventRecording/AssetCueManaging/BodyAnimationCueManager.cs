using System.Linq;
using Extensions;
using Models;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging
{
    internal sealed class BodyAnimationCueManager: IAssetCueManager
    {
        private readonly IEventEditor _eventEditor;
        private readonly BodyAnimationCueProvider _cueProvider;

        public BodyAnimationCueManager(IEventEditor eventEditor, BodyAnimationCueProvider cueProvider)
        {
            _eventEditor = eventEditor;
            _cueProvider = cueProvider;
        }

        public void SetupActivationCues(Event recordingEvent)
        {
            foreach (var characterController in recordingEvent.CharacterController)
            {
                var bodyAnimationController = characterController.GetBodyAnimationController();
                bodyAnimationController.ActivationCue = _cueProvider.GetActivationCue(recordingEvent, characterController);
            }

            if (_eventEditor.UseSameBodyAnimation)
            {
                UseTheActivationCueForAllCharacters(recordingEvent);
            }
        }
        
        public void SetupEndCues(Event recorded)
        {
            foreach (var characterController in recorded.CharacterController)
            {
                characterController.GetBodyAnimationController().EndCue =
                    _cueProvider.GetEndCue(characterController, recorded);
            }
        }

        private void UseTheActivationCueForAllCharacters(Event ev)
        {
            var bodyAnimController = ev.GetFirstCharacterController().GetBodyAnimationController();
            var targetCue = bodyAnimController.ActivationCue;
            SaveBodyAnimActivationCueForAllCharacters(ev, targetCue);
        }
        
        private void SaveBodyAnimActivationCueForAllCharacters(Event ev ,int activationCue)
        {
            foreach (var controller in ev.CharacterController)
            {
                var bodyAnimController = controller.GetBodyAnimationController();
                bodyAnimController.ActivationCue = activationCue;
            }
        }
    }
}
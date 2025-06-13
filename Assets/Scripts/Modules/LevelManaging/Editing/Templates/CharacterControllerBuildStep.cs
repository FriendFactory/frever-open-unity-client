using System.Collections.Generic;
using System.Linq;
using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    internal sealed class CharacterControllerBuildStep: EventBuildStep
    {
        protected override void RunInternal()
        {
            Destination.CharacterController = new List<CharacterController>();
            
            foreach (var templateController in Template.CharacterController)
            {
                var destinationController = new CharacterController();
                ApplyCharacterController(templateController, destinationController);
                Destination.CharacterController.Add(destinationController);
            }
        }

        private void ApplyCharacterController(CharacterController template, CharacterController dest)
        {
            ApplyCharacters(template, dest);
            ApplyBodyAnimation(template, dest);
            ApplyVoiceController(template, dest);
            ApplyMetaData(template, dest);
        }

        private void ApplyBodyAnimation(CharacterController template, CharacterController dest)
        {
            var templateBodyAnim = template.GetBodyAnimationController();
            var destBodyAnim = dest.GetBodyAnimationController();
            if (destBodyAnim == null)
            {
                destBodyAnim = new CharacterControllerBodyAnimation();
                dest.SetBodyAnimationController(destBodyAnim);
            }

            destBodyAnim.Locomotion = templateBodyAnim.Locomotion;
            destBodyAnim.Looping = templateBodyAnim.Looping;
            destBodyAnim.ActivationCue = templateBodyAnim.ActivationCue;
            destBodyAnim.EndCue = templateBodyAnim.EndCue;
            destBodyAnim.AnimationSpeed = templateBodyAnim.AnimationSpeed;
            
            destBodyAnim.SetBodyAnimation(templateBodyAnim.GetBodyAnimation());
        }

        private void ApplyVoiceController(CharacterController template, CharacterController dest)
        {
            var templateController = template.GetCharacterControllerFaceVoiceController();
            if(templateController == null) return;
            
            var destController = dest.GetCharacterControllerFaceVoiceController();
            if (destController == null)
            {
                destController = new CharacterControllerFaceVoice();
                dest.SetFaceVoiceController(destController);
            }

            destController.VoiceSoundVolume = templateController.VoiceSoundVolume;

            if (templateController.GetVoiceFilter() == null)
            {
                destController.SetVoiceFilter(null);
            }
            else
            {
                destController.SetVoiceFilter(templateController.GetVoiceFilter());
            }
        }

        private void ApplyMetaData(CharacterController template, CharacterController dest)
        {
            dest.ActivationCue = template.ActivationCue;
            dest.EndCue = template.EndCue;
            dest.ControllerSequenceNumber = template.ControllerSequenceNumber;
            dest.CharacterSpawnPositionId = template.CharacterSpawnPositionId;
        }

        private void ApplyCharacters(CharacterController template, CharacterController dest)
        {
            var templateCharacterId = template.CharacterId;
            var replaceCharacterData = ReplaceCharactersData.First(x => x.OriginCharacterId == templateCharacterId);
            var replaceCharacter = replaceCharacterData.ReplaceByCharacter;
            dest.Character = replaceCharacter;
            dest.CharacterId = replaceCharacter.Id;
            
            if(replaceCharacterData.ChangedOutfit == null) return;
            dest.SetOutfit(replaceCharacterData.ChangedOutfit);
        }
    }
}
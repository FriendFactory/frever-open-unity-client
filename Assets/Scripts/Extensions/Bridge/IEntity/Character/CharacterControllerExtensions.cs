using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions.Wardrobe;
using Models;
using UnityEngine;
using CharacterController = Models.CharacterController;

namespace Extensions
{
    public static class CharacterControllerExtensions
    {
        public static IEnumerable<CharacterController> GetControllersByCharacterId(this IEnumerable<CharacterController> characterControllers, long[] characterIds)
        {
            if (characterControllers == null) throw new ArgumentNullException(nameof(characterControllers));

            return characterControllers.Where(x=> characterIds.Contains(x.CharacterId));
        }
        
        public static void SetOutfitForAllControllers(this IEnumerable<CharacterController> characterControllers, OutfitFullInfo outfit)
        {
            if (characterControllers == null)
            {
                Debug.LogException(new Exception($"{nameof(characterControllers)} is null!"));
                return;
            }
            
            foreach (var cc in characterControllers)
            {
                cc.SetOutfit(outfit);
            }
        }
        
        public static void SetOutfit(this CharacterController characterController, OutfitFullInfo outfit)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            characterController.Outfit = outfit;
            characterController.OutfitId = outfit?.Id;
        }

        public static CharacterControllerBodyAnimation GetBodyAnimationController(this CharacterController characterController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            return characterController.CharacterControllerBodyAnimation?.FirstOrDefault();
        }

        public static void SetBodyAnimationController(this CharacterController characterController, CharacterControllerBodyAnimation bodyAnimController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            if (characterController.CharacterControllerBodyAnimation == null)
            {
                characterController.CharacterControllerBodyAnimation = new List<CharacterControllerBodyAnimation>();
            }
            else
            {
                characterController.CharacterControllerBodyAnimation.Clear();
            }
            
            if(bodyAnimController==null) return;
            characterController.CharacterControllerBodyAnimation.Add(bodyAnimController);
        }
        
        public static CharacterControllerFaceVoice GetCharacterControllerFaceVoiceController(this CharacterController characterController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            return characterController.CharacterControllerFaceVoice.FirstOrDefault();
        }

        public static void SetFaceVoiceController(this CharacterController characterController, CharacterControllerFaceVoice controller)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            if (characterController.CharacterControllerFaceVoice == null)
            {
                characterController.CharacterControllerFaceVoice = new List<CharacterControllerFaceVoice>();
            }
            else
            {
                characterController.CharacterControllerFaceVoice.Clear();
            }
            
            if(controller==null) return;
            characterController.CharacterControllerFaceVoice.Add(controller);
        }

        public static BodyAnimationInfo GetBodyAnimation(this CharacterController characterController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));
            
            return characterController.GetBodyAnimationController()?.BodyAnimation;
        }

        public static VoiceFilterFullInfo GetVoiceFilter(this CharacterController characterController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));
            
            return characterController.GetCharacterControllerFaceVoiceController()?.VoiceFilter;
        }

        public static bool HasSameBodyAnimation(this CharacterController target, CharacterController compare)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (compare == null) return false;
            var targetBodyAnim = target.GetBodyAnimation();
            var compareBodyAnim = compare.GetBodyAnimation();
            return compareBodyAnim?.Id == targetBodyAnim?.Id;
        }

        public static bool HaveTheSameMultiCharacterAnimation(this ICollection<CharacterController> controls)
        {
            var bodyAnimations = controls.Select(x => x.GetBodyAnimation()).ToArray();
            return bodyAnimations.All(x => x.IsMultiCharacter()) &&
                   bodyAnimations.DistinctBy(x => x.BodyAnimationGroupId).Count() == 1;
        }
        
        public static long GetBodyAnimationId(this CharacterController characterController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            return characterController.GetBodyAnimationController().BodyAnimationId;
        }
        
        public static FaceAnimationFullInfo GetFaceAnimation(this CharacterController characterController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            return characterController.GetCharacterControllerFaceVoiceController()?.FaceAnimation;
        }
        
        public static VoiceTrackFullInfo GetVoiceTrack(this CharacterController characterController)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            return characterController.GetCharacterControllerFaceVoiceController()?.VoiceTrack;
        }

        public static void SetCharacter(this CharacterController characterController, CharacterFullInfo character)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));
            characterController.Character = character;
            characterController.CharacterId = character.Id;
        }

        public static void SetBodyAnimation(this CharacterController characterController, BodyAnimationInfo bodyAnimation)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            var controller = characterController.GetBodyAnimationController();
            controller.SetBodyAnimation(bodyAnimation);
        }

        public static void SetVoiceFilter(this CharacterController characterController, VoiceFilterFullInfo voiceFilter)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));
           
            var voiceController = characterController.GetCharacterControllerFaceVoiceController();
            if (voiceController == null)
            {
                voiceController = new CharacterControllerFaceVoice();
                characterController.SetFaceVoiceController(voiceController);
            }

            voiceController.VoiceFilter = voiceFilter;
            voiceController.VoiceFilterId = voiceFilter.Id;
        }

        public static void SetVoiceVolume(this CharacterController characterController, int volume)
        {
            if (characterController == null) throw new ArgumentNullException(nameof(characterController));

            var voiceController = characterController.GetCharacterControllerFaceVoiceController();
            voiceController.VoiceSoundVolume = volume;
        }

        public static bool HasSameSpawnPosition(this IEnumerable<CharacterController> controllers)
        {
            if (controllers == null) throw new ArgumentNullException(nameof(controllers));
            if (controllers.Count() <= 1) return true;
            return controllers.Select(x => x.CharacterSpawnPositionId).Distinct().Count() == 1;
        }
        
        public static CharacterAndOutfit GetCharacterAndOutfitData(this CharacterController controller)
        {
            var character = controller.Character;
            var outfitFullInfo = controller.Outfit;

            return new CharacterAndOutfit
            {
                Character = character,
                Outfit = outfitFullInfo
            };
        }
        
        public static IEnumerable<CharacterAndOutfit> GetCharacterAndOutfitDataUnique(this IEnumerable<CharacterController> controller)
        {
            return controller.Select(x => x.GetCharacterAndOutfitData()).DistinctBy(x => new
            {
                CharacterId = x.Character.Id,
                OutfitId = x.Outfit?.Id
            });
        }

        public static WardrobeFullInfo[] GetUsedWardrobes(this CharacterController controller)
        {
            if (controller.Outfit != null)
            {
                return controller.Outfit.GetWardrobesForGender(controller.Character.GenderId);
            }

            return controller.Character.Wardrobes != null ? controller.Character.Wardrobes.ToArray() : Array.Empty<WardrobeFullInfo>();
        }

        public static IEntity[] GetUsedWardrobeEntities(this CharacterController controller)
        {
            var entities = new List<IEntity>();
            var wardrobes = controller.GetUsedWardrobes().Cast<IEntity>();
            
            entities.AddRange(wardrobes);
            
            if (controller.Outfit != null)
            {
                entities.Add(controller.Outfit.ToShortInfo());
            }

            return entities.ToArray();
        }
    }
}

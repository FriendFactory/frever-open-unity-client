using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Models;
using UnityEngine;
using CharacterController = Models.CharacterController;
using Event = Models.Event;

namespace Extensions
{
    /// <summary>
    ///     Character related
    /// </summary>
    public static partial class EventExtensions
    {
        public static void SetCharacterSpawnPosition(this Event @event, CharacterSpawnPositionInfo characterSpawnPosition)
        {
            if (@event == null)
            {
                Debug.LogException(new Exception($"{nameof(@event)} is null!"));
                return;
            }

            if (characterSpawnPosition == null) @event.CharacterSpawnPositionId = 0;
            else @event.CharacterSpawnPositionId = characterSpawnPosition.Id;
        }

        public static void UpdateCharacterSequenceNumbers(this Event @event)
        {
            var orderedCharacterControllers = @event.GetOrderedCharacterControllers();

            for (int i = 0; i < orderedCharacterControllers.Length; i++)
            {
                orderedCharacterControllers[i].ControllerSequenceNumber = i;
            }
        }

        public static CharacterController[] GetCharacterControllersByCharactersIds(this Event ev,
            params long[] charactersIds)
        {
            ThrowExceptionIfEventIsNull(ev);
            if (charactersIds == null || charactersIds.Length == 0) return null;
            if (ev.CharacterController == null || ev.CharacterController.Count == 0) return null;

            var characterControllers = new List<CharacterController>();

            foreach (var id in charactersIds)
            {
                var characterController =
                    ev.CharacterController.FirstOrDefault(cc => cc.CharacterId == id);

                if (characterController == null) continue;

                characterControllers.Add(characterController);
            }

            return characterControllers.ToArray();
        }
        
        public static CharacterFullInfo[] GetCharacters(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Select(x => x.Character).ToArray();
        }
        
        public static CharacterController[] GetOrderedCharacterControllers(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.OrderBy(x => x.ControllerSequenceNumber).ToArray();
        }

        public static int GetUniqueCharactersCount(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Select(x=>x.CharacterId).Distinct().Count();
        }
        
        public static int GetCharactersCount(this Event ev)
        {
            return ev.GetUniqueCharactersCount();
        }

        public static CharacterController GetCharacterController(this Event ev, int characterSequenceNumber)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.FirstOrDefault(x => x.ControllerSequenceNumber == characterSequenceNumber);
        }
        
        public static CharacterController GetCharacterControllerByControllerId(this Event ev, long controllerId)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.FirstOrDefault(x => x.Id == controllerId);
        }
        
        public static CharacterController GetCharacterControllerByCharacterId(this Event ev, long characterId)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.FirstOrDefault(x => x.CharacterId == characterId);
        }
        
        public static CharacterController GetCharacterControllerBySequenceNumber(this Event ev, int sequenceNumber)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.FirstOrDefault(x => x.ControllerSequenceNumber == sequenceNumber);
        }
        
        public static CharacterController[] GetCharacterControllersWithBodyAnimationId(this Event ev, long animationId)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Where(x => x.GetBodyAnimation().Id == animationId).ToArray();
        }
        
        public static CharacterController GetFirstCharacterController(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.FirstOrDefault();
        }

        public static FaceAnimationFullInfo[] GetFaceAnimations(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            
            return ev.CharacterController
                .Select(x => x.GetCharacterControllerFaceVoiceController())
                .Where(x=>x?.FaceAnimation != null)
                .Select(x => x.FaceAnimation).ToArray();
        }
        
        public static VoiceTrackFullInfo[] GetVoiceTracks(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            
            return ev.CharacterController
                .Select(x => x.GetCharacterControllerFaceVoiceController())
                .Where(x=>x?.VoiceTrack != null)
                .Select(x => x.VoiceTrack).ToArray();
        }
        
        public static CharacterControllerFaceVoice[] GetCharacterFaceVoiceControllers(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Where(x => x.CharacterControllerFaceVoice != null).SelectMany(x => x.CharacterControllerFaceVoice).ToArray();
        }
        
        public static CharacterControllerBodyAnimation[] GetCharacterBodyAnimationControllers(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.SelectMany(x => x.CharacterControllerBodyAnimation).ToArray();
        }
        
        public static CharacterControllerBodyAnimation[] GetCharacterBodyAnimationControllersWithAnimationId(this Event ev, long id)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Where(x => x.GetBodyAnimationId() == id).SelectMany(x=>x.CharacterControllerBodyAnimation).ToArray();
        }

        public static long[] GetUniqueBodyAnimationIds(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.SelectMany(x => x.CharacterControllerBodyAnimation).Select(x => x.BodyAnimationId).Distinct().ToArray();
        }
        
        public static long[] GetOutfitIds(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            var uniqueIds = ev.CharacterController.Where(x => x.OutfitId != null).Select(x => x.OutfitId).ToArray();
            return uniqueIds.Cast<long>().ToArray();
        }
        
        public static long[] GetCharacterIdsWithFaceAnimationId(this Event ev, long faceAnimId)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Where(x=>x.GetFaceAnimation()!=null).
                Where(x => x.GetFaceAnimation().Id == faceAnimId).
                Select(x=>x.CharacterId).ToArray();
        }
        
        public static long[] GetUniqueCharacterIds(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Select(x => x.CharacterId).Distinct().ToArray();
        }  
        public static VoiceFilterFullInfo GetVoiceFilter(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetTargetCharacterController().GetVoiceFilter();
        }

        public static int CharactersCount(this Event ev)
        {
            return ev.CharacterController.Count;
        }

        public static bool AreCharactersOnTheSameSpawnPosition(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CharacterController.Select(x => x.CharacterSpawnPositionId).Distinct().Count() < 2;
        }

        public static bool HasSetupMultiCharacterAnimation(this Event ev)
        {
            return ev.GetCharacterCountUsedTheSameMultiCharacterAnimation() > 1;
        }

        public static long GetMultiCharacterAnimationGroupId(this Event ev)
        {
            var groupsUsedInEvent = ev.CharacterController.Select(x => x.GetBodyAnimation().BodyAnimationGroupId)
                                      .Distinct().ToArray();
            if (groupsUsedInEvent.Count() != 1 || groupsUsedInEvent.First() == null)
            {
                throw new InvalidOperationException($"Event does not have single multiple character animation");
            }

            return groupsUsedInEvent.First().Value;
        }

        public static int GetCharacterCountUsedTheSameMultiCharacterAnimation(this Event ev)
        {
            var usedBodyAnimations = ev.CharacterController.Select(x => x.GetBodyAnimation());
            var multiCharacterAnimations = usedBodyAnimations.Where(x => x.BodyAnimationGroupId.HasValue);
            if (!multiCharacterAnimations.Any()) return 0;
            return multiCharacterAnimations.GroupBy(x => x.BodyAnimationGroupId.Value).Max(x => x.Count());
        }

        public static bool IsFormationApplied(this Event ev)
        {
            if (ev.GetCharactersCount() == 1) return false;
            return ev.CharacterController.GroupBy(x => x.CharacterSpawnPositionId).Count() == 1;
        }
    }
}
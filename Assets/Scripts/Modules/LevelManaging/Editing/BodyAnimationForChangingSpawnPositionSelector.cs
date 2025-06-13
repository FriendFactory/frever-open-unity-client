using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Models;

namespace Modules.LevelManaging.Editing
{
    internal struct ChangingSpawnPositionContext
    {
        public Event Event;
        public SetLocationFullInfo PreviousSpawnPointSetLocation;
        public SetLocationFullInfo NextSpawnPointSetLocation;
        public long NextSpawnPositionId;
    }
    
    /// <summary>
    /// Selects animations on spawn position changing (keeps existed or change to default)
    /// </summary>
    [UsedImplicitly]
    internal sealed class BodyAnimationForChangingSpawnPositionSelector: IBodyAnimationSelector<ChangingSpawnPositionContext>
    {
        private ChangingSpawnPositionContext Context { get; set; }
        
        private SetLocationFullInfo PreviousSpawnPointSetLocation => Context.PreviousSpawnPointSetLocation;
        private SetLocationFullInfo NextSpawnPointSetLocation => Context.NextSpawnPointSetLocation;
        private long NextSpawnPositionId => Context.NextSpawnPositionId;
        private Event Event => Context.Event;


        public void Setup(ChangingSpawnPositionContext selectorContext)
        {
            Context = selectorContext;
        }

        public SelectedAnimations Select()
        {
            var mainSpawnPosition = NextSpawnPointSetLocation.GetSpawnPosition(NextSpawnPositionId);
            
            var usingTheSameSpawnPositionForAllCharacters = !mainSpawnPosition.HasGroup() || Event.HasSetupMultiCharacterAnimation();
            if (usingTheSameSpawnPositionForAllCharacters)
            {
                return SelectBodyAnimationPerCharacterIfAllOfThemOnTheSameSpawnPosition(Event, PreviousSpawnPointSetLocation, mainSpawnPosition);
            }

            return SelectBodyAnimationPerCharacterIfTheyStayOnDifferentSpawnPositions(Event, PreviousSpawnPointSetLocation, NextSpawnPointSetLocation, mainSpawnPosition);
        }
        
        private static SelectedAnimations SelectBodyAnimationPerCharacterIfTheyStayOnDifferentSpawnPositions(Event ev, SetLocationFullInfo previousSpawnPositionSetLocation,
            SetLocationFullInfo nextSetLocation, CharacterSpawnPositionInfo nextMainSpawnPosition)
        {
            var characterToBodyAnimation = new Dictionary<long, long>();
            var characterToSpawnPosition = new Dictionary<long, long>();
            
            var groupedSpawnPositions = nextSetLocation.GetSpawnPositionsGroup(nextMainSpawnPosition.GetGroupId()).ToArray();
            var counter = 0;
            foreach (var controller in ev.GetOrderedCharacterControllers())
            {
                var previousPosition =
                    previousSpawnPositionSetLocation.GetSpawnPosition(controller.CharacterSpawnPositionId);
                var nextPosition = groupedSpawnPositions[counter];
                if (ShouldApplyDefaultAnimation(controller.GetBodyAnimation(), previousPosition,nextPosition))
                {
                    var characterId = controller.CharacterId;
                    characterToBodyAnimation[characterId] = nextPosition.DefaultBodyAnimationId.Value;
                    characterToSpawnPosition[characterId] = nextPosition.Id;
                }

                counter++;
            }

            return new SelectedAnimations
            {
                CharacterToSpawnPosition = characterToSpawnPosition,
                CharacterToBodyAnimation = characterToBodyAnimation
            };
        }

        private SelectedAnimations SelectBodyAnimationPerCharacterIfAllOfThemOnTheSameSpawnPosition(Event ev, SetLocationFullInfo currentSetLocation,
            CharacterSpawnPositionInfo nextSpawnPosition)
        {
            var characterToBodyAnimation = new Dictionary<long, long>();
            var characterToSpawnPosition = new Dictionary<long, long>();
            
            foreach (var characterController in ev.CharacterController)
            {
                var bodyAnim = characterController.GetBodyAnimation();
                var previousSpawnPos =
                    currentSetLocation.GetSpawnPosition(characterController.CharacterSpawnPositionId);
                if (ShouldApplyDefaultAnimation(bodyAnim, previousSpawnPos,nextSpawnPosition))
                {
                    var characterId = characterController.CharacterId;
                    characterToBodyAnimation[characterId] = nextSpawnPosition.DefaultBodyAnimationId.Value;
                    characterToSpawnPosition[characterId] = nextSpawnPosition.Id;
                }
            }

            return new SelectedAnimations
            {
                CharacterToSpawnPosition = characterToSpawnPosition,
                CharacterToBodyAnimation = characterToBodyAnimation
            };
        }

        private static bool ShouldApplyDefaultAnimation(BodyAnimationInfo bodyAnimation, CharacterSpawnPositionInfo previousSpawnPosition, CharacterSpawnPositionInfo nextSpawnPoint)
        {
            if (!nextSpawnPoint.DefaultBodyAnimationId.HasValue) return false;
            if (bodyAnimation.Id == nextSpawnPoint.DefaultBodyAnimationId) return false;
            if (!bodyAnimation.MovementTypeId.HasValue) return true;
            var animMovementType = bodyAnimation.MovementTypeId.Value;
            if (!nextSpawnPoint.SecondaryMovementTypeIds.IsNullOrEmpty() 
              && nextSpawnPoint.SecondaryMovementTypeIds.Contains(animMovementType))
            {
                return false;
            }

            if (!nextSpawnPoint.KeepAnimationCategoryIds.IsNullOrEmpty() &&
                nextSpawnPoint.KeepAnimationCategoryIds.Contains(bodyAnimation.BodyAnimationCategoryId))
            {
                return false;
            }

            if (bodyAnimation.MovementTypeId != nextSpawnPoint.MovementTypeId) return true;
            return bodyAnimation.Id == previousSpawnPosition.DefaultBodyAnimationId;
        }
    }
}
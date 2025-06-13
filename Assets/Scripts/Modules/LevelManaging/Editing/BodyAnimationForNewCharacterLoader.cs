using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Models;

namespace Modules.LevelManaging.Editing
{
    [UsedImplicitly]
    internal sealed class BodyAnimationForNewCharacterLoader
    {
        private readonly BodyAnimationForSpawnPositionLoader _bodyAnimationForSpawnPositionLoader;
        private readonly BodyAnimationModelsLoader _bodyAnimationModelsLoader;

        public bool IsLoading { get; private set; }
        public BodyAnimationInfo LoadedAnimation { get; private set; }

        public BodyAnimationForNewCharacterLoader(BodyAnimationForSpawnPositionLoader bodyAnimationForSpawnPositionLoader, BodyAnimationModelsLoader bodyAnimationModelsLoader)
        {
            _bodyAnimationForSpawnPositionLoader = bodyAnimationForSpawnPositionLoader;
            _bodyAnimationModelsLoader = bodyAnimationModelsLoader;
        }

        public async Task StartFetchingBodyAnimationForNewSpawnedCharacter(Event targetEvent)
        {
            IsLoading = true;
            LoadedAnimation = targetEvent?.GetTargetCharacterController()?.GetBodyAnimation()
                           ?? await LoadDefaultAnimationForSpawnPosition(targetEvent);
            IsLoading = false;
        }

        private async Task<BodyAnimationInfo> LoadDefaultAnimationForSpawnPosition(Event targetEvent)
        {
            var eventFocusedSpawnPosition =
                targetEvent.GetSetLocation().GetSpawnPosition(targetEvent.CharacterSpawnPositionId);
            var nextPos = GetNextSpawnPosition(targetEvent, eventFocusedSpawnPosition);

            if (!nextPos.HasDefaultBodyAnimation())
            {
                return targetEvent.GetTargetCharacterController().GetBodyAnimation();
            }

            var model = await _bodyAnimationModelsLoader.Load(nextPos.DefaultBodyAnimationId.Value);
            
            _bodyAnimationForSpawnPositionLoader.LoadAnimations(new []{model});
            while (_bodyAnimationForSpawnPositionLoader.IsLoading)
            {
                await Task.Delay(100);
            }

            return _bodyAnimationForSpawnPositionLoader.LoadedAnimationModels.First();
        }

        private static CharacterSpawnPositionInfo GetNextSpawnPosition(Event targetEvent,
            CharacterSpawnPositionInfo eventFocusedSpawnPosition)
        {
            if (eventFocusedSpawnPosition.HasGroup())
            {
                var group = targetEvent.GetSetLocation()
                                       .GetSpawnPositionsGroup(eventFocusedSpawnPosition.GetGroupId());
                return group.Skip(targetEvent.GetCharactersCount()).First();
            }

            return eventFocusedSpawnPosition;
        }
    }
}
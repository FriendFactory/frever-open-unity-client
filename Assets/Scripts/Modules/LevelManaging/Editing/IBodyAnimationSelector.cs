using System.Collections.Generic;
using Bridge.Models.ClientServer.Level.Shuffle;
using Extensions;
using JetBrains.Annotations;
using Models;

namespace Modules.LevelManaging.Editing
{
    internal interface IBodyAnimationSelector
    {
         SelectedAnimations Select();
    }

    internal interface IBodyAnimationSelector<in TContext>: IBodyAnimationSelector
    {
        void Setup(TContext context);
    }

    internal struct SelectedAnimations
    {
        public Dictionary<long, long> CharacterToBodyAnimation;
        public Dictionary<long, long> CharacterToSpawnPosition;
    }

    [UsedImplicitly]
    internal sealed class ShuffleAnimationsSelector: IBodyAnimationSelector<ShuffleContext>
    {
        private ShuffleContext Context { get; set; }
        private EventShuffleResult ShuffleResult => Context.ShuffleResult;

        public void Setup(ShuffleContext context)
        {
            Context = context;
        }

        public SelectedAnimations Select()
        {
            var characterToSpawnPosition = new Dictionary<long, long>();
            var characterToBodyAnimation = new Dictionary<long, long>();

            var orderedControllers = Context.TargetEvent.GetOrderedCharacterControllers();
            for (var i = 0; i < orderedControllers.Length; i++)
            {
                var settings = ShuffleResult.Characters[i];
                var spawnPositionId = settings.CharacterSpawnPositionId;
                var controller = orderedControllers[i];
                var characterId = controller.CharacterId;
                characterToSpawnPosition[characterId] = spawnPositionId;
                characterToBodyAnimation[characterId] = settings.BodyAnimationId;
            }

            return new SelectedAnimations
            {
                CharacterToSpawnPosition = characterToSpawnPosition,
                CharacterToBodyAnimation = characterToBodyAnimation
            };
        }
    }

    internal struct ShuffleContext
    {
        public EventShuffleResult ShuffleResult;
        public Event TargetEvent;
    }
}
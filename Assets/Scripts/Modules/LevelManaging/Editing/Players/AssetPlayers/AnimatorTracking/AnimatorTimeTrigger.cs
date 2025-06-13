using System;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking
{
    public class AnimatorTimeTrigger
    {
        public long AnimationId { get; }
        public float TriggerTime { get; }
        public Action OnTrigger { get; }

        public bool IsTriggered { get; set; }

        public AnimatorTimeTrigger(long animationId, float triggerTime, Action onTrigger)
        {
            AnimationId = animationId;
            TriggerTime = triggerTime;
            OnTrigger = onTrigger;
            IsTriggered = false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AnimationId, TriggerTime);
        }
    }
}
using Modules.Haptics;
using UnityEngine;
using Zenject;

namespace UIManaging.Common.Buttons
{
    public abstract class EventBasedHapticFeedback<TSelectable>: EventBasedFeedback<TSelectable> where TSelectable : Component
    {
        [SerializeField] private HapticFX _effect = HapticFX.Low;

        [Inject] private IHapticFeedbackManager _hapticFeedbackManager;

        protected override void PlayFeedback()
        {
            _hapticFeedbackManager.PlayEffect(_effect);
        }

        protected abstract override void Subscribe();
        protected abstract override void Unsubscribe();
    }
}
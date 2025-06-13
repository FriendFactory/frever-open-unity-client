using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Buttons
{
    [RequireComponent(typeof(Toggle))]
    internal class ToggleHapticFeedback: EventBasedHapticFeedback<Toggle>
    {
        protected override void Subscribe()
        {
            Source.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void Unsubscribe()
        {
            Source.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool _)
        {
            PlayFeedback();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Buttons
{
    [RequireComponent(typeof(Button))]
    internal class ButtonHapticFeedback: EventBasedHapticFeedback<Button>
    {
        protected override void Subscribe()
        {
            Source.onClick.AddListener(PlayFeedback);
        }

        protected override void Unsubscribe()
        {
            Source.onClick.RemoveListener(PlayFeedback);
        }
    }
}
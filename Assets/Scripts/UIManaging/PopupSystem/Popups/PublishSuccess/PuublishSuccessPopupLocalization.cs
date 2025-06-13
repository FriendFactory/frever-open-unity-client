using I2.Loc;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    [CreateAssetMenu(menuName = "L10N/Popups/PublishSuccess", fileName = "PublishSuccessPopupLocalization")]
    internal sealed class PublishSuccessPopupLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _shareAvailableButtonLabel;
        [SerializeField] private LocalizedString _shareUnavailableButtonLabel;
        [SerializeField] private LocalizedString _dailyVideoSharingProgress;

        public string ShareAvailableButtonLabel => _shareAvailableButtonLabel;
        public string ShareUnavailableButtonLabel => _shareUnavailableButtonLabel;
        public string DailyVideoSharingProgress => _dailyVideoSharingProgress;
    }
}

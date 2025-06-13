using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(fileName = "StorePageLocalization", menuName = "L10N/StorePageLocalization", order = 0)]
    public class StorePageLocalization : ScriptableObject
    {
        [Header("Support Creator")] 
        [SerializeField] private LocalizedString _supportCreatorHeader;
        [SerializeField] private LocalizedString _supportCreatorErrorMessage;
        [SerializeField] private LocalizedString _supportCreatorSuccessMessage;
        [SerializeField] private LocalizedString _supportCreatorDefaultSupportMessage;
        [Header("Cancel Creator Support")] 
        [SerializeField] private LocalizedString _cancelSupportTitle;
        [SerializeField] private LocalizedString _cancelSupportDescription;
        [SerializeField] private LocalizedString _cancelSupportConfirm;
        [SerializeField] private LocalizedString _cancelSupportCancel;
        [SerializeField] private LocalizedString _cancelSupportFailedMessage;
        [Header("Premium Pass")]
        [SerializeField] private LocalizedString _premiumPassTimeLeft;

        public LocalizedString SupportCreatorHeader => _supportCreatorHeader;
        public LocalizedString SupportCreatorErrorMessage => _supportCreatorErrorMessage;
        public LocalizedString SupportCreatorSuccessMessage => _supportCreatorSuccessMessage;
        public LocalizedString SupportCreatorDefaultSupportMessage => _supportCreatorDefaultSupportMessage;
        public LocalizedString CancelSupportTitle => _cancelSupportTitle;
        public LocalizedString CancelSupportDescription => _cancelSupportDescription;
        public LocalizedString CancelSupportConfirm => _cancelSupportConfirm;
        public LocalizedString CancelSupportCancel => _cancelSupportCancel;
        public LocalizedString PremiumPassTimeLeft => _premiumPassTimeLeft;
        public LocalizedString CancelSupportFailedMessage => _cancelSupportFailedMessage;
    }
}
using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class ManageAccountPageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeader;
        [Space]
        [SerializeField] private LocalizedString _deletePopupTitle;
        [SerializeField] private LocalizedString _deletePopupDesc;
        [SerializeField] private LocalizedString _deletePopupConfirm;
        [SerializeField] private LocalizedString _deletePopupCancel;
        [Space]
        [SerializeField] private LocalizedString _revokeAppleTokenPopupTitle;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string PageHeader => _pageHeader;

        public string DeletePopupTitle => _deletePopupTitle;
        public string DeletePopupDesc => _deletePopupDesc;
        public string DeletePopupConfirm => _deletePopupConfirm;
        public string DeletePopupCancel => _deletePopupCancel;

        public string RevokeAppleTokenPopupTitle => _revokeAppleTokenPopupTitle;
    }
}
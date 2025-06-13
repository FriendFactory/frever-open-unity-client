using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.ContactsPage
{
    internal sealed class ContactsPageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeader;
        [Space]
        [SerializeField] private LocalizedString _findPopupTitle;
        [SerializeField] private LocalizedString _findPopupDesc;
        [SerializeField] private LocalizedString _findPopupConfirm;
        [SerializeField] private LocalizedString _findPopupCancel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string PageHeader => _pageHeader;

        public string FindPopupTitle => _findPopupTitle;
        public string FindPopupDesc => _findPopupDesc;
        public string FindPopupConfirm => _findPopupConfirm;
        public string FindPopupCancel => _findPopupCancel;
    }
}
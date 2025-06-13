using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.DraftsPage
{
    internal sealed class DraftsPageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeader;
        [Space]
        [SerializeField] private LocalizedString _deletingDraftsMessage;
        [SerializeField] private LocalizedString _draftDeletedMessage;
        [SerializeField] private LocalizedString _draftsDeletedMessage;
        [Space]
        [SerializeField] private LocalizedString _deletePopupTitle;
        [SerializeField] private LocalizedString _deletePopupDesc;
        [SerializeField] private LocalizedString _deletePopupPositive;
        [SerializeField] private LocalizedString _deletePopupNegative;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string PageHeader => _pageHeader;

        public string DeletingDraftsMessage => _deletingDraftsMessage;
        public string DraftDeletedMessage => _draftDeletedMessage;
        public string DraftsDeletedMessage => _draftsDeletedMessage;

        public string DeletePopupTitle => _deletePopupTitle;
        public string DeletePopupDesc => _deletePopupDesc;
        public string DeletePopupPositive => _deletePopupPositive;
        public string DeletePopupNegative => _deletePopupNegative;
    }
}
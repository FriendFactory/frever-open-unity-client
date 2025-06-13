using EnhancedUI.EnhancedScroller;
using Modules.Contacts;

namespace UIManaging.Pages.ContactsPage.CellViews
{
    internal abstract class ContactsCellView : EnhancedScrollerCellView
    {
        public abstract ContactType Type { get; }

        private void Awake()
        {
            cellIdentifier = GetType().Name;
        }

        public abstract void Initialize(ContactsItemModel model);
    }
}

using Navigation.Core;

namespace UIManaging.Pages.ContactsPage
{
    public class ContactsPageArgs : PageArgs
    {
        private int _initialTabIndex;
        public override PageId TargetPage { get; } = PageId.ContactsPage;

        public int InitialTabIndex
        {
            get => _initialTabIndex;
            private set => _initialTabIndex = value;
        }

        public ContactsPageArgs()
        {
        }

        public ContactsPageArgs(int initialTabIndex)
        {
            InitialTabIndex = initialTabIndex;
        }

        public void SetInitialTabIndex(int index)
        {
            InitialTabIndex = index;
        }
    }
}

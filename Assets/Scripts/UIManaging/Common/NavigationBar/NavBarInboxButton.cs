using Navigation.Args;
using Navigation.Core;

namespace UIManaging.Common
{
    public sealed class NavBarInboxButton : NavBarButtonBase
    {
        protected override void OnButtonClicked()
        {
            if (PageManager.IsChangingPage) return;

            PageManager.MoveNext(PageId.Inbox, new InboxPageArgs()); }
    }
}
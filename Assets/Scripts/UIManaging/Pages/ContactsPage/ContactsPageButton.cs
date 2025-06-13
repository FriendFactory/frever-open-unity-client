using Navigation.Core;
using UIManaging.Core;

namespace UIManaging.Pages.ContactsPage
{
    public class ContactsPageButton : ButtonBase
    {
        protected override void OnClick()
        {
            var args = new ContactsPageArgs();
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true
            };
            
            Manager.MoveNext(PageId.ContactsPage, args, transitionArgs);
        }
    }
}

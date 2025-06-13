using Modules.Contacts;
using UIManaging.Pages.ContactsPage.ItemViews;

namespace UIManaging.Pages.ContactsPage.CellViews
{
    internal sealed class ContactWithAccountCellView : GenericContactsCellView<ContactWithAccountItemView, ContactWithAccountItemModel>
    {
        public override ContactType Type => ContactType.WithAccount;
    }
}

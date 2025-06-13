using Modules.Contacts;
using UIManaging.Pages.ContactsPage.ItemViews;

namespace UIManaging.Pages.ContactsPage.CellViews
{
    internal sealed class ContactWithoutAccountCellView :  GenericContactsCellView<ContactWithoutAccountItemView, ContactWithoutAccountItemModel>
    {
        public override ContactType Type => ContactType.WithoutAccount;
    }
}

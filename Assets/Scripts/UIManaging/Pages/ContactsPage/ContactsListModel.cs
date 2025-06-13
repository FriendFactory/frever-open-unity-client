using Modules.Contacts;

namespace UIManaging.Pages.ContactsPage
{
    public sealed class ContactsListModel
    {
        public ContactsItemModel[] Contacts { get; private set; }

        public ContactsListModel(ContactsItemModel[] contacts)
        {
            Contacts = contacts;
        }

    }
}

using System;
using VoxelBusters.CoreLibrary;

namespace Modules.Contacts
{
    public interface IAddressBookContactsProvider
    {
        bool HasLoaded { get; }
        
        ContactsItemModel[] Contacts { get; }

        void ReadContacts(bool onlyFreverContacts = false, Action<ContactsItemModel[]> onComplete = null,
            Action<Error> onFail = null);

        void ReadContactsWithPermission(bool onlyFreverContacts = false, Action<ContactsItemModel[]> onComplete = null,
            Action<Error> onFail = null);
    }
}
using System.Linq;
using Modules.Contacts;
using VoxelBusters.EssentialKit;

namespace Modules.Contacts
{
    internal static class ContactsModelFactory
    {
        internal static ContactWithoutAccountItemModel GetContactWithoutAccount(IAddressBookContact contact)
        {
            var firstName = contact.FirstName;
            var lastName = contact.LastName;
            var contactNumber = contact.PhoneNumbers.FirstOrDefault();

            return new ContactWithoutAccountItemModel(firstName, lastName, contactNumber);
        }

        internal static ContactWithAccountItemModel GetContactWithAccount(IAddressBookContact contact, long groupId)
        {
            var firstName = contact.FirstName;
            var lastName = contact.LastName;

            return new ContactWithAccountItemModel(firstName, lastName, groupId);
        }
    }
}
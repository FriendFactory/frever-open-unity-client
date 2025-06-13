namespace Modules.Contacts
{
    public sealed class ContactWithoutAccountItemModel : ContactsItemModel
    {
        public string PhoneNumber { get;}
        public override ContactType Type => ContactType.WithoutAccount;
        public ContactWithoutAccountItemModel(string name, string lastName, string phoneNumber) : base(name, lastName)
        {
            PhoneNumber = phoneNumber;
        }
    }
}

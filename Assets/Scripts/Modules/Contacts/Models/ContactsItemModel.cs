namespace Modules.Contacts
{
    public abstract class ContactsItemModel
    {
        public abstract ContactType Type { get; }
        public string Name { get; }
        public string LastName { get; }

        public ContactsItemModel(string name, string lastName)
        {
            Name = name;
            LastName = lastName;
        }
    }
}

using System;

namespace Modules.Contacts
{
    public sealed class ContactWithAccountItemModel : ContactsItemModel
    {
        public Action<long> OnFollow;
        public Action<long> OnUnfollow;
        public bool IsFollowed;
        public override ContactType Type => ContactType.WithAccount;
        public long GroupId { get;}
        public ContactWithAccountItemModel(string name, string lastName, long groupId) : base(name, lastName)
        {
            GroupId = groupId;
        }
        
    }
}

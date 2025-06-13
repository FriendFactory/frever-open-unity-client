using Modules.Contacts;
using UnityEngine;

namespace UIManaging.Pages.ContactsPage.ItemViews
{
    internal sealed class ContactWithoutAccountItemView : ContactsItemView<ContactWithoutAccountItemModel>
    {
        [SerializeField] private InviteContactButton _inviteContactButton;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SubText.text = ContextData.PhoneNumber;
            _inviteContactButton.Initialize(ContextData.PhoneNumber);
        }
    }
}

using Abstract;
using Modules.Contacts;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.ContactsPage.ItemViews
{
    internal abstract class ContactsItemView<T> : BaseContextDataView<T> where T: ContactsItemModel
    {
        [SerializeField] protected TextMeshProUGUI ContactNameText;
        [SerializeField] protected TextMeshProUGUI SubText;
        protected override void OnInitialized()
        {
            ContactNameText.text = ContextData.Name + " " + ContextData.LastName;
        }
    }
}


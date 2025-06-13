using Modules.Contacts;
using UIManaging.Pages.ContactsPage.ItemViews;

namespace UIManaging.Pages.ContactsPage.CellViews
{
    internal class GenericContactsCellView<T, N> : ContactsCellView where  N: ContactsItemModel where T : ContactsItemView<N>
    {
        public override ContactType Type { get; }

        public override void Initialize(ContactsItemModel model)
        {
            var view = GetComponent<T>();
            var convertedModel = model as N;
            view.Initialize(convertedModel);
        }
    }
}

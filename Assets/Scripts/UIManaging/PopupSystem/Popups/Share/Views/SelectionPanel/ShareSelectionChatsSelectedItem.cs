namespace UIManaging.PopupSystem.Popups.Share
{
    internal abstract class ShareSelectionChatsSelectedItem: ShareSelectionSelectedItem<ShareSelectionChatsItemModel>
    {
        protected override string Title => ContextData.Title;
    }
}
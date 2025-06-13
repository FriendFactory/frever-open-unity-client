namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionCategoryModel : IShareSelectionItemModel
    {
        public ShareSelectionCategoryModel(string title)
        {
            Title = title;
        }

        public string Title { get; }
    }
}
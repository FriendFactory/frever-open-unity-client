using System;

namespace UIManaging.Common.SelectableGrid
{
    public class DeletableGridManagerArgs
    {
        public string Title { get; }
        public Action<long[]> OnDeleteButtonClicked  { get; }
        public Action OnCloseButtonClicked  { get; }

        public DeletableGridManagerArgs(string title, Action<long[]> onDeleteButtonClicked, Action onCloseButtonClicked)
        {
            Title = title;
            OnDeleteButtonClicked = onDeleteButtonClicked;
            OnCloseButtonClicked = onCloseButtonClicked;
        }
    }
}
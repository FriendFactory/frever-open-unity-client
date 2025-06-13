using System.Collections.Generic;

namespace UIManaging.PopupSystem.Popups.Share
{
    public interface IShareSelectionListModel
    {
        IReadOnlyList<IShareSelectionItemModel> Items { get; }
    }
}
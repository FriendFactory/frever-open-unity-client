using System;
using System.Collections.Generic;

namespace UIManaging.Pages.InboxPage.Interfaces
{
    public interface IChatListModel
    {
        event Action ItemsChanged;

        IReadOnlyList<IChatItemModel> Items { get; }

        void RequestPage();
    }
}
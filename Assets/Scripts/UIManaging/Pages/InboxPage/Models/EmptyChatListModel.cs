using System;
using System.Collections.Generic;
using UIManaging.Pages.InboxPage.Interfaces;

namespace UIManaging.Pages.InboxPage.Models
{
    public class EmptyChatListModel: IChatListModel
    {
        #pragma warning disable CS0067
        public event Action ItemsChanged;
        #pragma warning restore CS0067

        public IReadOnlyList<IChatItemModel> Items => new List<IChatItemModel>();
        
        public void RequestPage()
        {
            
        }
    }
}
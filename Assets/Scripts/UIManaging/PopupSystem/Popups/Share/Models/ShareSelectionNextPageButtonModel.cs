using System;

namespace UIManaging.PopupSystem.Popups.Share
{
    public enum SearchButtonType
    {
        Chats,
        Friends
    }

    public enum SearchNextButtonCommand
    {
        Enable,
        Disable,
        SetBusy
    }

    public enum SearchNextButtonState
    {
        Enabled,
        Disabled,
        Busy
    }

    public class ShareSelectionNextPageButtonModel : IShareSelectionItemModel
    {
        public SearchNextButtonState State { get; private set; }
        public string Name { get; }
        
        public event Action Clicked;
        public event Action<SearchNextButtonState> StateChanged;

        public ShareSelectionNextPageButtonModel(string name)
        {
            Name = name;
            State = SearchNextButtonState.Enabled;
        }

        public void Click()
        {
            Clicked?.Invoke();
        }

        public void ChangeState(SearchNextButtonState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
    
    public class ShareSelectionChatsNextPageButtonModel: ShareSelectionNextPageButtonModel
    {
        public ShareSelectionChatsNextPageButtonModel(string name) : base(name) { }
    }
    
    public class ShareSelectionFriendsNextPageButtonModel: ShareSelectionNextPageButtonModel
    {
        public ShareSelectionFriendsNextPageButtonModel(string name) : base(name) { }
    }
}
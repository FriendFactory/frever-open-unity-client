using Bridge.Models.ClientServer.Chat;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionChatsItemModel : ShareSelectionItemModel
    {
        public ShareSelectionChatsItemModel(ChatShortInfo chatShortInfo, bool isSelected): base(isSelected)
        {
            ChatShortInfo = chatShortInfo;
        }

        public ChatShortInfo ChatShortInfo { get; }
        
        public override long Id => ChatShortInfo.Id;
        public override string Title => ChatShortInfo?.Title;
    }

    internal class ShareSelectionGroupChatsItemModel : ShareSelectionChatsItemModel
    {
        public ShareSelectionGroupChatsItemModel(ChatShortInfo chatShortInfo, bool isSelected = false) : base(chatShortInfo, isSelected) { }
    }

    internal class ShareSelectionDirectChatsItemModel : ShareSelectionChatsItemModel
    {
        public ShareSelectionDirectChatsItemModel(ChatShortInfo chatShortInfo, bool isSelected = false) : base(chatShortInfo, isSelected) { }
    }
    
    internal class ShareSelectionCrewChatsItemModel: ShareSelectionChatsItemModel
    {
        public ShareSelectionCrewChatsItemModel(ChatShortInfo chatShortInfo, bool isSelected = false) : base(chatShortInfo, isSelected) { }
    }
}
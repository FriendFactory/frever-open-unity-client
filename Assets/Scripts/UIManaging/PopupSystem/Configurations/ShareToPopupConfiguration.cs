using System;
using Bridge.Models.ClientServer.Chat;
using Bridge.Services.UserProfile;
using Extensions;

namespace UIManaging.PopupSystem.Configurations
{
    public class ShareToPopupConfiguration: PopupConfiguration
    {
        public long? VideoId { get; }
        public bool OwnVideo { get; }
        public string CompleteButtonText { get; }
        public Action<ShareDestinationData> OnConfirmed { get; }
        
        public ShareDestinationData PreselectedDestinationData { get; }
        public bool BlockConfirmButtonIfNoSelection { get; }
        public Action OnCancelled { get; }

        public ShareToPopupConfiguration(Action<ShareDestinationData> onConfirmed, bool ownVideo, string completeButtonText, long? videoId = null, Action<object> onClose = null, string title = "", ShareDestinationData preselectedDestinationData = null, bool blockConfirmButtonIfNoSelection = false, Action onCancelled = null)
            : base(PopupType.ShareTo, onClose, title)
        {
            VideoId = videoId;
            OwnVideo = ownVideo;
            OnConfirmed = onConfirmed;
            OnCancelled = onCancelled;
            BlockConfirmButtonIfNoSelection = blockConfirmButtonIfNoSelection;
            CompleteButtonText = completeButtonText;
            PreselectedDestinationData = preselectedDestinationData;
        }
    }

    public sealed class ShareDestinationData
    {
        public ChatShortInfo[] Chats;
        public Profile[] Users;

        public bool HasAny => !Chats.IsNullOrEmpty() || !Users.IsNullOrEmpty();
    }
}
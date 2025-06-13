using System;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using UIManaging.Common;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    public abstract class MessageItemModel : UserTimestampItemModel
    {
        private readonly Action<ChatMessage> _onReply;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<MessageItemModel> OnLongPress;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Type CachedType { get; private set; }

        public long ChatId { get; }
        public ChatType ChatType { get; }
        public ChatMessage ChatMessage { get; }
        public string Username { get; }

        public Action<Texture2D> OnPhotoThumbnailClicked { get; }
        public Action<Video> OnVideoThumbnailClicked { get; }

        public bool HasPendingSizeChange { get; set; }

        public virtual MessageType MessageType => ChatMessage.GetMessageType();
        public string CommentText => ChatMessage.Text;

        public bool IsLikedByCurrentUser
        {
            get => ChatMessage.IsLikedByCurrentUser;
            internal set => ChatMessage.IsLikedByCurrentUser = value;
        }

        public long LikeCount
        {
            get => ChatMessage.LikeCount;
            internal set => ChatMessage.LikeCount = value;
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected MessageItemModel(long chatId, ChatType chatType, ChatMessage chatMessage, string username,
            Action<ChatMessage> onReply, Action<Texture2D> onPhotoThumbnailClicked, Action<Video> onVideoThumbnailClicked)
            : base(chatMessage.Group?.Id ?? 0, chatMessage.Time)
        {
            CachedType = GetType();

            ChatId = chatId;
            ChatType = chatType;

            ChatMessage = chatMessage;
            Username = username;
            _onReply = onReply;
            OnPhotoThumbnailClicked = onPhotoThumbnailClicked;
            OnVideoThumbnailClicked = onVideoThumbnailClicked;

            // By default, the model's size is unknown, so mark it for size re-calculation
            HasPendingSizeChange = true;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OpenContextMenu() => OnLongPress?.Invoke(this);

        public void OnReply()
        {
            if (!ChatMessage.IsMessageType(MessageType.User)) return;
            _onReply?.Invoke(ChatMessage);
        }
    }
}
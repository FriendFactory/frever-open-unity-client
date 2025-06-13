using System;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    public class SystemMessageModel : MessageItemModel
    {
        public SystemMessageModel(

                long chatId,
                ChatType chatType,
                ChatMessage chatMessage,
                string username,
                Action<ChatMessage> onReply,
                Action<Texture2D> onPhotoThumbnailClicked,
                Action<Video> onVideoThumbnailClicked

            ) : base (

                chatId,
                chatType,
                chatMessage,
                username,
                onReply,
                onPhotoThumbnailClicked,
                onVideoThumbnailClicked
            )
        {
        }
    }
}
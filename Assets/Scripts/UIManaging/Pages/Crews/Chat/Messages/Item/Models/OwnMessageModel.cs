using System;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    public class OwnMessageModel : MessageItemModel
    {
        public OwnMessageModel(

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

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override MessageType MessageType => MessageType.Own;
    }
}
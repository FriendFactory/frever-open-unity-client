using System;
using System.Collections.Generic;
using Bridge.VideoServer;
using UIManaging.Common;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.Comments
{
    public class CommentItemModel : UserTimestampItemModel
    {
        private const float DEFAULT_MIN_HEIGHT = 232;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<CommentItemModel> OnReply;
        public event Action<CommentItemModel> OnLongPress;
        public event Action<CommentItemModel> OnLike;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public CommentInfo CommentInfo { get; }
        public bool HeightAdjusted { get; set; }
        public bool FadeInOnLoading { get; set; }
        public long Id => CommentInfo.Id;
        public string CommentText => CommentInfo.Text;
        public bool IsRoot => CommentInfo.ReplyToComment == null;
        public bool IsReplyToRoot => CommentInfo.Key.Split('.').Length == 2;
        public string NickName => CommentInfo.GroupNickname;
        public CommentGroupInfo ReplyInfo => CommentInfo.ReplyToComment;
        public float ItemHeight { get; set; } = DEFAULT_MIN_HEIGHT;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CommentItemModel(CommentInfo commentInfo) : base(commentInfo.GroupId, commentInfo.Time)
        {
            CommentInfo = commentInfo;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ReplyToComment() => OnReply?.Invoke(this);

        public void OpenContextMenu() => OnLongPress?.Invoke(this);

        public void LikeComment()
        {
            CommentInfo.IsLikedByCurrentUser = !CommentInfo.IsLikedByCurrentUser;
            if (CommentInfo.IsLikedByCurrentUser) CommentInfo.LikeCount += 1;
            else CommentInfo.LikeCount -= 1;
            OnLike?.Invoke(this);
        }
    }
}
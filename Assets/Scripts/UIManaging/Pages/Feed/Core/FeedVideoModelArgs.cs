using System;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Events;

namespace UIManaging.Pages.Feed.Core
{
    public sealed class FeedVideoModelArgs
    {
        public Video Video;
        public  MediaPathType FileLocation;
        public bool CanUseAsTemplate;
        public bool ShowBasedOnTemplateButton;
        public bool IsNavBarActive;
        public bool IsPostedDateAndViewsAmountActive;
        public HashtagInfo HashtagInfo;
        public long TaskId;
        public ScaleMode? ScaleMode;
        public Action FollowButtonClick;
        public UnityAction JoinTaskButtonClick;
        public UnityAction OnJoinTemplateClick { get; set; }
        public bool ShowActionsBar;
        public bool ShowVideoDescription;
        public bool ShowChallengeButtonInDescription;
        public IPlayableMusic Sound { get; set; }
    }
}

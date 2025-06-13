using System;
using System.Text;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using RenderHeads.Media.AVProVideo;
using UIManaging.Pages.Feed.Core;
using UnityEngine;
using UnityEngine.Events;
using Video = Bridge.Models.VideoServer.Video;

namespace Modules.VideoStreaming.Feed
{
    public class FeedVideoModel
    {
        private static readonly StringBuilder STRING_BUILDER = new StringBuilder();

        public UnityAction JoinTaskButtonClick;
        public Action FollowButtonClick;
        public UnityAction OnJoinTemplateClick;
        
        public string Url => Video.RedirectUrl;
        public bool CanUseForRemix { get; }
        public bool ShowBasedOnTemplateButton { get; }
        public MediaPathType FileLocation { get; }
        public Video Video { get; }
        public bool IsVideoAvailable { get; set; }
        public bool IsNavbarActive { get; }
        public bool IsPostedDateAndViewsAmountActive { get; }
        public readonly HashtagInfo OpenedWithHashtag;
        public readonly long OpenedWithTask;
        public VideoAccess VideoAccess => Video?.Access ?? VideoAccess.Public;
        public ScaleMode? ScaleMode;
        public bool ShowActionsBar { get; }
        public bool ShowVideoDescription { get; }
        public bool ShowChallengeButtonInDescription { get; }
        public IPlayableMusic OpenedWithSound { get; }
        
        
        private HttpHeader? _header;

        public HttpHeader HttpHeader
        {
            get
            {
                if (_header == null)
                {
                    _header = SetupHeader();
                }
                return _header.Value;
            }
        }

        public FeedVideoModel(FeedVideoModelArgs args)
        {
            Video = args.Video;
            FileLocation = args.FileLocation;
            CanUseForRemix = args.CanUseAsTemplate;
            ShowBasedOnTemplateButton = args.ShowBasedOnTemplateButton;
            OpenedWithHashtag = args.HashtagInfo;
            OpenedWithTask = args.TaskId;
            IsNavbarActive = args.IsNavBarActive;
            IsPostedDateAndViewsAmountActive = args.IsPostedDateAndViewsAmountActive;
            FollowButtonClick = args.FollowButtonClick;
            JoinTaskButtonClick = args.JoinTaskButtonClick;
            OnJoinTemplateClick = args.OnJoinTemplateClick;
            ScaleMode = args.ScaleMode;
            ShowActionsBar = args.ShowActionsBar;
            ShowVideoDescription = args.ShowVideoDescription;
            ShowChallengeButtonInDescription = args.ShowChallengeButtonInDescription;
            OpenedWithSound = args.Sound;
        }

        public override string ToString()
        {
            return $"FileLocation: {FileLocation}, Url: {Url}, PostedDateUtc: {Video.CreatedTime}, Likes: {Video.KPI.Likes}, SharesAmount: {Video.KPI.Shares}, " +
                   $"ViewsAmount: {Video.KPI.Views}, RemixesAmount: {Video.KPI.Remixes}";
        }
        
        private HttpHeader SetupHeader()
        {
            STRING_BUILDER.Clear();

            foreach (var cook in Video.SignedCookies)
            {
                STRING_BUILDER.Append($"{cook.Key}={cook.Value}; ");
            }

            return new HttpHeader("Cookie", STRING_BUILDER.ToString());
        }
    }
}

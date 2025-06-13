using System.Collections.Generic;
using System.Linq;
using Bridge.Models.VideoServer;
using Bridge.NotificationServer;
using Navigation.Args.Feed;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;

namespace Modules.VideoStreaming.Feed
{
    public class FeedManagerArgs
    {
        public int IndexOfFirstVideoToShow => FeedVideoModels.IndexOf(FirstVideoToShow);
        public List<FeedVideoModel> FeedVideoModels { get; }
        public long? IdOfFirstVideoToShow { get; }
        public bool CanUseVideosAsTemplate { get; }
        public bool ShowBasedOnTemplateButton { get; }
        public bool IsNavBarActive { get; }
        
        public string FeedName { get; }
        public VideoListType FeedType { get; }
        public CommentInfo CommentInfo { get; }
        public HashtagInfo HashtagInfo { get; }
        public bool ShowViews { get; set; }
        public bool ShowActionsBar { get; set; }
        public bool ShowVideoDescription { get; set; }
        public ScaleMode? ScaleMode { get; set; }

        private FeedVideoModel FirstVideoToShow => FeedVideoModels.FirstOrDefault(video => video.Video.Id == IdOfFirstVideoToShow);

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        internal FeedManagerArgs(List<FeedVideoModel> feedVideoModels, long? idOfFirstVideoToShow, bool showViews, BaseFeedArgs feedPageArgs)
        {
            FeedVideoModels = feedVideoModels;
            IdOfFirstVideoToShow = idOfFirstVideoToShow;
            CanUseVideosAsTemplate = feedPageArgs.CanUseVideosAsTemplate;
            FeedName = feedPageArgs.Name;
            FeedType = feedPageArgs.VideoListType;
            ShowBasedOnTemplateButton = feedPageArgs.ShouldShowUseBasedOnTemplateButton();
            CommentInfo = feedPageArgs.CommentInfo;
            HashtagInfo = (feedPageArgs as HashtagFeedArgs)?.HashtagInfo;
            IsNavBarActive = feedPageArgs.ShouldShowNavigationBar();
            ShowViews = showViews;
            ShowActionsBar = feedPageArgs.ShowActionsBar;
            ShowVideoDescription = feedPageArgs.ShowVideoDescription;
            ScaleMode = feedPageArgs.ScaleMode;
        }
    }
}
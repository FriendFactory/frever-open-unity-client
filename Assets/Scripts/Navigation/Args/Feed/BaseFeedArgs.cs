using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge.NotificationServer;
using Modules.Amplitude;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.Events;

namespace Navigation.Args.Feed
{
    public abstract class BaseFeedArgs : PageArgs
    {
        private readonly Dictionary<VideoListType, long?> _lastShownVideosTypes;

        protected readonly VideoManager VideoManager;
        public int VideoModelIndex;
        
        public Action FollowedAccount;
        public Action JoinedChallenge;
        public Action FirstVideoLoaded;
        public Action PageLoaded;
        public UnityAction OnJoinTemplateClick;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId TargetPage => AmplitudeManager.IsGamifiedFeedEnabled() ? PageId.GamifiedFeed : PageId.Feed;
        public virtual bool CanUseVideosAsTemplate => false;
        public long? IdOfFirstVideoToShow { get; private set; }
        public bool FindVideoWithRemix { get; set; }
        public abstract string Name { get; }
        public CommentInfo CommentInfo { get; }
        public int VideoModelsCount { get; set; }
        public virtual VideoListType VideoListType { get; set; } = AmplitudeManager.IsForMeFeatureEnabled() ? VideoListType.ForMe : VideoListType.Featured;
        public virtual bool BlockScrolling => false;
        public virtual bool ShowActionsBar => true;
        public virtual bool ShowBackButton => true;
        public virtual bool ShowQuestsButton => true;
        public virtual bool ShowVideoDescription => true;
        public virtual ScaleMode ScaleMode => ScaleMode.ScaleAndCrop;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected BaseFeedArgs(VideoManager videoManager)
        {
            VideoManager = videoManager;
            _lastShownVideosTypes = new Dictionary<VideoListType, long?>();
        }
        
        protected BaseFeedArgs(VideoManager videoManager, long? idOfFirstVideoToShow,
                               CommentInfo commentInfo = null, UnityAction onJoinTemplateClick = null) : this(videoManager)
        {
            IdOfFirstVideoToShow = idOfFirstVideoToShow;
            SetLastShownForVideoListType(VideoListType, idOfFirstVideoToShow);
            CommentInfo = commentInfo;
            OnJoinTemplateClick = onJoinTemplateClick;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetIdOfFirstVideoToShow(long? idOfFirstVideoToShow)
        {
            IdOfFirstVideoToShow = idOfFirstVideoToShow;
            SetLastShownForVideoListType(VideoListType, idOfFirstVideoToShow);
        }
        
        public void SetVideoListType(VideoListType type)
        {
            VideoListType = type;
        }

        public void SetLastShownForVideoListType(VideoListType tabIndex, long? videoId)
        {
            _lastShownVideosTypes[tabIndex] = videoId;
        }
        
        public long? GetLastShownVideoListType(VideoListType type)
        {
            return _lastShownVideosTypes.ContainsKey(type) ? _lastShownVideosTypes[type] : null;
        }
        
        public void DownloadVideos(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            DownloadVideosInternal(OnSuccess, OnFail, videoId, takeNextCount, takePreviousCount, cancellationToken);

            void OnSuccess(Video[] videos)
            {
                OnVideosDownloaded(videos, onSuccess);
            }

            void OnFail(string message)
            {
                onFail?.Invoke(message);
            }
        }

        public virtual bool ShouldShowTabs()
        {
            return false;
        }
        
        public virtual bool ShouldShowNavigationBar()
        {
            return true;
        }
        
        public virtual bool ShouldShowUseBasedOnTemplateButton()
        {
            return true;
        }

        public virtual bool ShouldShowNotificationButton()
        {
            return false;
        }

        public virtual bool ShouldShowDiscoveryButton()
        {
            return false;
        }

        public virtual bool ShouldRefreshOnVideoDeleted()
        {
            return true;
        }

        public virtual void OnVideoDeleted()
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual void OnVideosDownloaded(Video[] videos, Action<Video[]> callback)
        {
            videos = OnBeforeVideosCallback(videos);
            callback?.Invoke(videos);
        }

        protected virtual Video[] OnBeforeVideosCallback(Video[] inputVideos)
        {
            return inputVideos.OrderByDescending(video => video.CreatedTime).ToArray();
        }

        protected abstract void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default);
    }
}
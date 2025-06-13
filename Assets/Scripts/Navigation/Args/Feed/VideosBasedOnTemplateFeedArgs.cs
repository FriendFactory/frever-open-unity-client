using System;
using System.Threading;
using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;

namespace Navigation.Args.Feed
{
    public class VideosBasedOnTemplateFeedArgs : BaseFeedArgs
    {
        private readonly long _templateId;

        public VideosBasedOnTemplateFeedArgs(VideoManager videoManager, long templateId) : base(videoManager)
        {
            _templateId = templateId;
        }
        
        public VideosBasedOnTemplateFeedArgs(VideoManager videoManager, long idOfFirstVideoToShow,
            int indexOfFirstVideoToShow, long templateId, Action OnJoinTemplateClick = null) : base(videoManager, idOfFirstVideoToShow)
        {
            _templateId = templateId;
            VideoModelIndex = indexOfFirstVideoToShow;
        }

        public override string Name => "Based on Template";

        protected override async void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            if (takeNextCount == 0) return;

            var videosToSkip = (VideoModelIndex == 0 || VideoModelsCount == 0)
                ? Mathf.Clamp(VideoModelIndex - takePreviousCount, 0, int.MaxValue)
                : Mathf.Clamp(VideoModelIndex + takeNextCount - takePreviousCount - 1, 0, int.MaxValue);

            var result = await VideoManager.GetVideoForTemplate(_templateId, videoId, takeNextCount + takePreviousCount, videosToSkip, cancellationToken);
            if (result.IsSuccess)
            {
                onSuccess?.Invoke(result.Video);
            }
            else if(!result.IsCanceled)
            {
                onFail?.Invoke(result.ErrorMessage);
            }
        }
        
        protected override void OnVideosDownloaded(Video[] videos, Action<Video[]> callback)
        {
            SetTargetTemplateIdAsMain(videos);
            callback?.Invoke(videos);
        }
        
        public override bool ShouldShowTabs()
        {
            return false;
        }

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }
        
        public override bool ShouldShowUseBasedOnTemplateButton()
        {
            return false;
        }

        //To make sure correct template will be loaded when user wants to use them in when pressing "Join to Record" in feed. 
        private void SetTargetTemplateIdAsMain(Video[] videos)
        {
            foreach (var video in videos)
            {
                video.MainTemplate.Id = _templateId;
            }
        }
    }
}
using System;
using System.Threading;
using Bridge.Models.VideoServer;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;

namespace Navigation.Args.Feed
{
    public sealed class ChatVideoFeedArgs : BaseFeedArgs
    {
        private readonly Video _video;
        private readonly PageManager _pageManager;

        public override string Name => "ChatVideoFeed";
        public override bool ShowQuestsButton => false;
        public override bool BlockScrolling => true;
        public override bool ShowActionsBar => !_video.IsPublishedAsMessage();
        public override bool ShowVideoDescription => !_video.IsPublishedAsMessage();
        public override ScaleMode ScaleMode => _video.IsPublishedAsMessage() ? ScaleMode.ScaleToFit : ScaleMode.ScaleAndCrop;

        public ChatVideoFeedArgs(VideoManager videoManager, PageManager pageManager, Video video) : base(videoManager, video.Id)
        {
            _pageManager = pageManager;
            _video = video;
        }

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancelationToken = default)
        {
            onSuccess?.Invoke(new[] {_video});
        }

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }

        public override bool ShouldRefreshOnVideoDeleted()
        {
            return false;
        }

        public override void OnVideoDeleted()
        {
            _pageManager.MoveBack();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class RemixFeedArgs : BaseFeedArgs
    {
        private readonly IBridge _bridge;
        private readonly long _videoId;
        private readonly long? _remixedFromVideoId;
        private Video _rootOriginalVideo;
        
        public override bool CanUseVideosAsTemplate => true;
        public override string Name => "Remix";

        public RemixFeedArgs(long? remixedFromVideoId, long videoId, IBridge bridge, VideoManager videoManager, long idOfFirstVideoToShow) : base(videoManager, idOfFirstVideoToShow)
        {
            _remixedFromVideoId = remixedFromVideoId;
            _videoId = videoId;
            _bridge = bridge;
        }

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? targetVideoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            var resultVideoList = new List<Video>();
            
            VideoManager.GetRemixVideoListAsync(OnRemixVideoDownloaded, onFail, _videoId, targetVideoId, takeNextCount, takePreviousCount, cancellationToken);

            void OnRemixVideoDownloaded(Video[] videos)
            {
                resultVideoList.AddRange(videos);

                if (_remixedFromVideoId.HasValue && resultVideoList.Count > 0)
                {
                    GetRootOriginalVideo(OnRootOriginalVideoDownloaded);
                }
                else
                {
                    onSuccess?.Invoke(resultVideoList.ToArray());
                }
            }

            void OnRootOriginalVideoDownloaded(Video rootOriginalVideo)
            {
                _rootOriginalVideo = rootOriginalVideo;
                onSuccess?.Invoke(resultVideoList.ToArray());
            }
        }

        protected override Video[] OnBeforeVideosCallback(Video[] inputVideos)
        {
            var inputVideosList = base.OnBeforeVideosCallback(inputVideos).ToList();
            
            if(_rootOriginalVideo != null)
            {
                inputVideosList.Insert(0,_rootOriginalVideo);
                _rootOriginalVideo = null;
            }
            
            return inputVideosList.ToArray();
        }

        private async void GetRootOriginalVideo(Action<Video> callback)
        {
            var id = _remixedFromVideoId.Value;
            var getRootOriginalVideoTask = _bridge.GetVideoAsync(id);
            await getRootOriginalVideoTask;
            
            if (getRootOriginalVideoTask.Result.IsSuccess)
            {
                callback?.Invoke(getRootOriginalVideoTask.Result.ResultObject);
            }
            else
            {
                callback?.Invoke(null);
            }
        }

        public override bool ShouldShowTabs()
        {
            return false;
        }

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }
    }
}
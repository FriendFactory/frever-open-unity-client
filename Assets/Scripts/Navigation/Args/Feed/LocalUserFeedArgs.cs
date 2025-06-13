using System;
using System.Threading;
using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class LocalUserFeedArgs : BaseFeedArgs
    {
        public override string Name => "LocalUser";
        public override VideoListType VideoListType => VideoListType.Profile;

        public LocalUserFeedArgs(VideoManager videoManager, long idOfFirstVideoToShow) : base(videoManager, idOfFirstVideoToShow)
        {
 
        }

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancelationToken = default)
        {
            VideoManager.GetVideosForLocalUser(onSuccess, onFail, videoId, takeNextCount, takePreviousCount, cancelationToken);
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
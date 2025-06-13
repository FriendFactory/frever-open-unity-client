using System;
using System.Threading;
using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class RemoteUserFeedArgs : BaseFeedArgs
    {
        private readonly long _userGroupId;
        
        public override string Name => "RemoteUser";

        public RemoteUserFeedArgs(VideoManager videoManager, long userGroupId, long idOfFirstVideoToShow) : base(videoManager, idOfFirstVideoToShow)
        {
            _userGroupId = userGroupId;
        }

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            VideoManager.GetVideosForRemoteUser(onSuccess, onFail, _userGroupId, videoId, takeNextCount, takePreviousCount, cancellationToken);
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
using System;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge.NotificationServer;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class UserTaskFeedArgs : BaseFeedArgs
    {
        private readonly long _userGroupId;

        public UserTaskFeedArgs(long userGroupId, VideoManager videoManager, long idOfFirstVideoToShow, CommentInfo commentInfo = null) : base(videoManager, idOfFirstVideoToShow, commentInfo)
        {
            _userGroupId = userGroupId;
        }

        public override string Name => "UserTask";

        public override bool ShouldShowTabs()
        {
            return false;
        }

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }

        public override bool ShouldShowNotificationButton()
        {
            return false;
        }

        public override bool ShouldShowUseBasedOnTemplateButton()
        {
            return true;
        }

        protected override async void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            await VideoManager.GetUserVideoForTasks(videoId, _userGroupId, takeNextCount, takePreviousCount, cancellationToken, onSuccess, onFail);
        }
    }
}
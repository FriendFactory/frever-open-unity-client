using System;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge.NotificationServer;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class TaskFeedArgs : BaseFeedArgs
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override string Name => "Tasks";
        public override VideoListType VideoListType => VideoListType.Task;
        public long TaskId { get; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public TaskFeedArgs(VideoManager videoManager, long taskId) : base(videoManager)
        {
            TaskId = taskId;
        }

        public TaskFeedArgs(VideoManager videoManager, long idOfFirstVideoToShow, long taskId, CommentInfo commentInfo = null) : base(videoManager, idOfFirstVideoToShow, commentInfo)
        {
            TaskId = taskId;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }

        public override bool ShouldShowTabs()
        {
            return false;
        }

        public override bool ShouldShowUseBasedOnTemplateButton()
        {
            return true;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId,
                                                       int takeNextCount,
                                                       int takePreviousCount = 0,
                                                       CancellationToken cancellationToken = default)
        {
            VideoManager.GetTaskVideos(TaskId, videoId, takeNextCount, takePreviousCount, onSuccess, onFail, cancellationToken);
        }

        protected override Video[] OnBeforeVideosCallback(Video[] inputVideos)
        {
            return inputVideos;
        }
    }
}
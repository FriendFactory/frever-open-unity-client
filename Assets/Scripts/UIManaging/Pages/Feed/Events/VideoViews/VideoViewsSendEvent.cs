using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.Feed.Events.VideoViews
{
    internal enum VideoViewsSendEventReason
    {
        PageChanged = 0,
        TabChanged = 1,
        TimeReached = 2,
        UnsentEvents = 3,
    }

    internal sealed class VideoViewsSendEvent
    {
        public VideoViewsSendEventReason Reason { get; }
        public string FeedType { get; }
        public VideoListType FeedTab { get; }

        public VideoViewsSendEvent(VideoViewsSendEventReason reason, string feedName, VideoListType feedTab)
        {
            Reason = reason;
            FeedType = feedName;
            FeedTab = feedTab;
        }
    }
}
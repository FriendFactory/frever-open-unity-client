using System;

namespace UIManaging.Pages.Feed.Events.VideoViews
{
    internal class VideoViewData
    {
        public long VideoId { get; }
        public DateTime Timestamp { get; }
        
        public VideoViewData(long videoId, DateTime timestamp)
        {
            VideoId = videoId;
            Timestamp = timestamp;
        }
    }
}
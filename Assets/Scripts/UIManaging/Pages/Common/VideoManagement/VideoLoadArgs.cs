using System.Threading;

namespace UIManaging.Pages.Common.VideoManagement
{
    public class VideoLoadArgs
    {
        public int TakeNext { get; set; }
        public int TakePrevious { get; set; }
        public string TargetVideoKey { get; set; }
        public long? TargetVideoId { get; set; }
        public virtual VideoListType VideoType { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
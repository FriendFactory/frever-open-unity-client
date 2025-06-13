namespace UIManaging.Common
{
    public class TemplateThumbnail : VideoThumbnail
    {
        protected override void Awake()
        {
            VideoPlayerPlaybackMode = new VideoPlayerModeDelayedLoop();
        }
    }
}
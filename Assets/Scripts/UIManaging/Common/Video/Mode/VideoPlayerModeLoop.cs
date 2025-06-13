namespace UIManaging.Common
{
    public class VideoPlayerModeLoop : VideoPlayerPlaybackMode
    {
        public override void StartPlayback()
        {
            MediaPlayer.Loop = true;
            base.StartPlayback();
        }
    }
}
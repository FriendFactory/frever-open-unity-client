using RenderHeads.Media.AVProVideo;

namespace UIManaging.Common
{
    public abstract class VideoPlayerPlaybackMode
    {        
        public MediaPlayer MediaPlayer { get; set; }

        public virtual void StartPlayback()
        {
            MediaPlayer.Play();
        }

        public virtual void StopPlayback()
        {
            MediaPlayer.Pause();
        }
    }
}
using Abstract;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace UIManaging.Pages.RatingFeed
{
    public class RatingFeedVideoPlayer: BaseContextDataView<RatingFeedVideoPlayerModel>
    {
        [SerializeField] protected DisplayUGUI _displayUgui;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public MediaPlayer MediaPlayer
        {
            get => _displayUgui.CurrentMediaPlayer;
            set => _displayUgui.CurrentMediaPlayer = value;
        }

        public bool IsReadyToPlay { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            ContextData?.PlayerVisible(this);
        }

        private void OnDisable()
        {
            ContextData?.PlayerDisabled(this);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            if (string.IsNullOrEmpty(ContextData.VideoUrl) || IsDestroyed) return;

            if (gameObject.activeInHierarchy)
            {
                ContextData?.PlayerVisible(this);
            }

            IsReadyToPlay = false;
            SetupMediaPlayer();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ContextData?.PlayerCancelled(this);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupMediaPlayer()
        {
            var mediaPlayer = _displayUgui.CurrentMediaPlayer;

            // In case user was scrolling profile page when video was published media player could be null
            if (mediaPlayer is null) return;

            mediaPlayer.PlaybackRate = 1;
            var path = new MediaPath(ContextData.VideoUrl, MediaPathType.AbsolutePathOrURL);
            mediaPlayer.Events.AddListener(OnEventInvoked);

            if (ContextData.HttpHeader.IsComplete())
            {
                mediaPlayer.GetCurrentPlatformOptions().httpHeaders.Clear();
                mediaPlayer.GetCurrentPlatformOptions().httpHeaders.Add(ContextData.HttpHeader.name, ContextData.HttpHeader.value);
            }

            mediaPlayer.OpenMedia(path, false);
        }

        private void OnEventInvoked(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            switch (eventType)
            {
                case MediaPlayerEvent.EventType.ReadyToPlay:
                    IsReadyToPlay = true;
                    break;
                case MediaPlayerEvent.EventType.Started:
                    MediaPlayer.Events.RemoveListener(OnEventInvoked);
                    break;

                case MediaPlayerEvent.EventType.Error:
                    ContextData.PlayerError(this);
                    MediaPlayer.Events.RemoveListener(OnEventInvoked);
                    break;
            }
        }
    }
}
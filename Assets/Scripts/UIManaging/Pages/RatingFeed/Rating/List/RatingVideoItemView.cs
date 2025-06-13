using System;
using Bridge.Models.VideoServer;
using Common.Abstract;
using Extensions;
using RenderHeads.Media.AVProVideo;
using UIManaging.Pages.RatingFeed.Signals;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class RatingVideoItemView: BaseContextPanel<RatingVideoItemModel>
    {
        private const float WIDE_ASPECT_RATIO = 18f / 9f;
        private const float WIDE_BOTTOM_OFFSET = 85f;

        [SerializeField] private RatingFeedVideoPlayer _videoPlayer;
        [SerializeField] private VideoRatingPanel _ratingPanel;
        [SerializeField] private RatingVideoDetailsPanel _detailsPanel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        [Inject] private SignalBus _signalBus;

        private Video Video => ContextData.RatingVideo.Video;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Start()
        {
            var currentScreenRatio = Screen.height / (float) Screen.width;
            if (currentScreenRatio < WIDE_ASPECT_RATIO)
            {
                ((RectTransform) _ratingPanel.transform).SetBottom(WIDE_BOTTOM_OFFSET);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        public void Play()
        {
            if (_videoPlayer.IsReadyToPlay)
            {
                StartPlaying();
            }
            else
            {
                _videoPlayer.MediaPlayer.Events.AddListener(OnMediaPlayerEvent);
            }
        }

        protected override void OnInitialized()
        {
            _ratingPanel.Initialize(ContextData.RatingVideo.Rating);
            _videoPlayer.Initialize(new RatingFeedVideoPlayerModel(Video.RedirectUrl, Video.SignedCookies));
            _detailsPanel.Initialize(Video);
        }

        protected override void BeforeCleanUp()
        {
            _videoPlayer.MediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
            _videoPlayer.MediaPlayer.Stop();

            _ratingPanel.CleanUp();
            _videoPlayer.CleanUp();
            _detailsPanel.CleanUp();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnMediaPlayerEvent(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            if (eventType != MediaPlayerEvent.EventType.ReadyToPlay) return;
            _videoPlayer.MediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
            StartPlaying();
        }

        private void StartPlaying()
        {
            _videoPlayer.MediaPlayer.Play();
            _ratingPanel.StartRatingCountdown();
            _signalBus.Fire(new RatingVideoStartedPlayingSignal(ContextData.RatingVideo, DateTime.UtcNow));
        }
    }
}
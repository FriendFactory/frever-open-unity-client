using Abstract;
using DG.Tweening;
using Navigation.Args;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace UIManaging.Common
{
    public class VideoThumbnail : BaseContextDataView<VideoThumbnailModel>
    {
        [SerializeField] protected DisplayUGUI _displayUgui;
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private CanvasGroup _displayUguiCanvasGroup;

        protected VideoPlayerPlaybackMode VideoPlayerPlaybackMode;

        public MediaPlayer MediaPlayer
        {
            get => _displayUgui.CurrentMediaPlayer;
            set => _displayUgui.CurrentMediaPlayer = value;
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            VideoPlayerPlaybackMode = new VideoPlayerModeDelayedLoop();
        }

        private void OnEnable()
        {
            ResetFadeAnimation();
            ContextData?.PlayerVisible(this);
        }

        private void OnDisable()
        {
            ContextData?.PlayerDisabled(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _displayUgui.DOKill();
            _displayUguiCanvasGroup.DOKill();
            _displayUguiCanvasGroup.alpha = 1;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetupMediaPlayer()
        {
            var mediaPlayer = _displayUgui.CurrentMediaPlayer;
            
            // In case user was scrolling profile page when video was published media player could be null 
            if(mediaPlayer is null) return;
            
            VideoPlayerPlaybackMode.MediaPlayer = _displayUgui.CurrentMediaPlayer;
            mediaPlayer.PlaybackRate = 1;
            var path = new MediaPath(ContextData.VideoUrl, MediaPathType.AbsolutePathOrURL);
            mediaPlayer.Events.AddListener(OnEventInvoked);

            if (ContextData.HttpHeader.IsComplete())
            {
                mediaPlayer.GetCurrentPlatformOptions().httpHeaders.Clear();
                mediaPlayer.GetCurrentPlatformOptions().httpHeaders.Add(ContextData.HttpHeader.name, ContextData.HttpHeader.value);
            }

            mediaPlayer.OpenMedia(path);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            if (string.IsNullOrEmpty(ContextData.VideoUrl) || IsDestroyed) return;

            StartedLoading();

            MediaPlayerPool.Instance.AddToInitializationQueue(ContextData);
            
            if (gameObject.activeInHierarchy)
            {
                ContextData?.PlayerVisible(this);
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ResetFadeAnimation();
            ContextData?.PlayerCancelled(this);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnEventInvoked(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            switch (eventType)
            {
                case MediaPlayerEvent.EventType.Started:
                    ShowPlayer();
                    MediaPlayer.Events.RemoveListener(OnEventInvoked);
                    break;
                
                case MediaPlayerEvent.EventType.Error:
                    ContextData.PlayerError(this);
                    MediaPlayer.Events.RemoveListener(OnEventInvoked);
                    break;
            }
        }

        private void ResetFadeAnimation()
        {
            _displayUguiCanvasGroup.DOKill();
            _displayUguiCanvasGroup.alpha = 0;
        }
        
        private void ShowPlayer()
        {
            VideoPlayerPlaybackMode.StartPlayback();
            _displayUguiCanvasGroup.DOKill();
            _displayUguiCanvasGroup.DOFade(1f, _fadeDuration).SetDelay(0.2f)
                                   .SetEase(Ease.InCirc);
        }

        private void StartedLoading()
        {
            _displayUguiCanvasGroup.DOKill();
            _displayUguiCanvasGroup.alpha = 0f;
        }
    }
}
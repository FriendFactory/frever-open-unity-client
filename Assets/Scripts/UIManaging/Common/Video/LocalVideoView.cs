using Abstract;
using DG.Tweening;
using RenderHeads.Media.AVProVideo;
using UIManaging.Common.Interfaces;
using UnityEngine;

namespace UIManaging.Common
{
    public class LocalVideoView : BaseContextDataView<ILocalVideoModel>
    {
        private const float FADE_DURATION = 0.25f;
        
        [SerializeField] private DisplayUGUI _displayUgui;
        [SerializeField] private CanvasGroup _displayUguiCanvasGroup;

        private VideoPlayerPlaybackMode _videoPlayerPlaybackMode;
        
        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _videoPlayerPlaybackMode = new VideoPlayerModeLoop();

            ContextData.VideoStarted += OnVideoStarted;
            
            if (ContextData.IsStarted)
            {
                _displayUguiCanvasGroup.alpha = 1;
                SetupMediaPlayer();
            }
            else
            {
                _displayUguiCanvasGroup.alpha = 0;
            }
        }

        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                ContextData.VideoStarted -= OnVideoStarted;
            }

            if (_displayUgui.CurrentMediaPlayer != null)
            {
                _displayUgui.CurrentMediaPlayer.CloseMedia();
            }
            
            base.BeforeCleanup();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnVideoStarted()
        {
            _displayUguiCanvasGroup.DOFade(1f, FADE_DURATION).SetEase(Ease.InCirc);
            SetupMediaPlayer();
        }
        
        private void SetupMediaPlayer()
        {
            var mediaPlayer = _displayUgui.CurrentMediaPlayer;
            
            _videoPlayerPlaybackMode.MediaPlayer = _displayUgui.CurrentMediaPlayer;
            
            var path = new MediaPath(ContextData.LocalFilePath, MediaPathType.AbsolutePathOrURL);
            
            mediaPlayer.PlaybackRate = 1;
            mediaPlayer.Events.AddListener(OnEventInvoked);
            mediaPlayer.OpenMedia(path);
        }
        
        private void OnEventInvoked(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            switch (eventType)
            {
                case MediaPlayerEvent.EventType.Started:
                    _videoPlayerPlaybackMode.StartPlayback();
                    player.Events.RemoveListener(OnEventInvoked);
                    break;
                
                case MediaPlayerEvent.EventType.Error:
                    ContextData.Error(errorCode.ToString());
                    player.Events.RemoveListener(OnEventInvoked);
                    break;
            }
        }
    }
}
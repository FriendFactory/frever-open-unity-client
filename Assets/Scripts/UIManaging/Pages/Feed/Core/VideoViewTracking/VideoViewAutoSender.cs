using System;
using System.Collections;
using JetBrains.Annotations;
using UIManaging.Pages.Feed.Ui.Feed;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Feed.Core.VideoViewTracking
{
    public sealed class VideoViewAutoSender : MonoBehaviour
    {
        private const int SEND_INTERVAL_TIME = 30;

        private VideoViewTracker _videoTacker;
        private VideoViewSender _videoViewSender;
        private VideoViewFileHandler _videoViewFileHandler;
        
        private bool _counterIsRunning;
        private DateTime _lastSentTime = DateTime.UtcNow;
        
        [Inject, UsedImplicitly]
        private void Construct(VideoViewTracker videoTacker, VideoViewSender videoViewSender, VideoViewFileHandler videoViewFileHandler)
        {
            _videoTacker = videoTacker;
            _videoViewSender = videoViewSender;
            _videoViewFileHandler = videoViewFileHandler; 
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _videoTacker.AddedVideoView += OnVideoTracked;
        }

        private void OnDestroy()
        {
            _videoTacker.AddedVideoView -= OnVideoTracked;
            var videos = _videoTacker.GetVideosViewedAfter(_lastSentTime);
            if (videos.Length > 0)
            {
                _videoViewFileHandler.Save(videos);
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Send()
        {
            var videos = _videoTacker.GetVideosViewedAfter(_lastSentTime);
            if (videos.Length <= 0) return;
            
            _videoViewSender.Send(videos,()=> _videoViewFileHandler.Save(videos));
            _lastSentTime = DateTime.UtcNow;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator StartSendCounter()
        {
            _counterIsRunning = true;
            yield return new WaitForSeconds(SEND_INTERVAL_TIME);

            Send();
            _counterIsRunning = false;
            _lastSentTime = DateTime.UtcNow;
        }

        private void OnVideoTracked()
        {
            if (!_counterIsRunning)
            {
                StartCoroutine(StartSendCounter());
            }
        }
    }
}

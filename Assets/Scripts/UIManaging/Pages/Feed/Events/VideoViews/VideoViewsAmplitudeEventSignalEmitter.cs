using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Modules.Amplitude;
using Modules.Amplitude.Signals;
using Modules.TempSaves.Manager;
using Modules.VideoStreaming.Feed;
using UIManaging.Pages.Feed.Core;
using Zenject;

namespace UIManaging.Pages.Feed.Events.VideoViews
{
    [UsedImplicitly]
    internal class VideoViewsAmplitudeEventSignalEmitter:  BaseAmplitudeEventSignalEmitter
    {
        private readonly FeedManagerView _feedManagerView;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly VideoViewsSendEventEmitter _sendEventEmitter;
        private readonly VideoViewsAmplitudeEventFileHandler _fileHandler;
        
        private readonly List<VideoViewData> _videoViews = new ();

        public VideoViewsAmplitudeEventSignalEmitter(FeedManagerView feedManagerView, SignalBus signalBus, VideoViewsSendEventEmitter sendEventEmitter, TempFileManager tempFileManager) : base(signalBus)
        {
            _feedManagerView = feedManagerView;
            _sendEventEmitter = sendEventEmitter;
            
            _fileHandler = new VideoViewsAmplitudeEventFileHandler(tempFileManager);
        }

        public override void Initialize()
        {
            _feedManagerView.OnVideoStartedPlaying += OnVideoStartPlaying;
            _sendEventEmitter.SendTriggered += OnSendTriggered;
            
            if (_fileHandler.TryLoad(out var unsentEvent))
            {
                Emit(unsentEvent);
            }
        }

        public override void Dispose()
        {
            _feedManagerView.OnVideoStartedPlaying -= OnVideoStartPlaying;
            _sendEventEmitter.SendTriggered -= OnSendTriggered;
        }

        private void OnVideoStartPlaying(FeedVideoModel feedVideoModel)
        {
            var timestamp = DateTime.UtcNow;
            
            _videoViews.Add(new VideoViewData(feedVideoModel.Video.Id, timestamp));
        }

        private void OnSendTriggered(VideoViewsSendEvent videoViewsSendEvent) => Send(videoViewsSendEvent);

        private void Send(VideoViewsSendEvent sendEvent)
        {
            if (_videoViews.Count == 0) return;

            var videoViewsEvent = new VideoViewsAmplitudeEvent(_videoViews, sendEvent.FeedType, sendEvent.FeedTab);

            if (sendEvent.Reason == VideoViewsSendEventReason.UnsentEvents)
            {
                _fileHandler.Save(videoViewsEvent);
                return;
            }
            
            _videoViews.Clear();
            
            Emit(videoViewsEvent);
        }
    }
}
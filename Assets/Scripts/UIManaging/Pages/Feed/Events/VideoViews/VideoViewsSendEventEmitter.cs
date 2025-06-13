using System;
using Common.ApplicationCore;
using JetBrains.Annotations;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Feed.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Feed.Events.VideoViews
{
    [UsedImplicitly]
    internal class VideoViewsSendEventEmitter: IInitializable, IDisposable, ITickable
    {
        private const float SEND_RATE = 30f;
        
        private readonly FeedPage _feedPage;
        private readonly FeedManagerView _feedManagerView;
        private readonly PageManager _pageManager;
        private readonly IAppEventsSource _appEventsSource;
        
        private float _nextSendTime;
        private VideoListType _currentVideoTabType;

        private bool IsFeed => _pageManager.IsCurrentPage(PageId.Feed) || _pageManager.IsCurrentPage(PageId.GamifiedFeed);
        
        public event Action<VideoViewsSendEvent> SendTriggered;

        public VideoViewsSendEventEmitter(FeedPage feedPage, FeedManagerView feedManagerView, PageManager pageManager, IAppEventsSource appEventsSource)
        {
            _feedPage = feedPage;
            _feedManagerView = feedManagerView;
            _pageManager = pageManager;
            _appEventsSource = appEventsSource;
        }

        public void Initialize()
        {
            _nextSendTime = Mathf.Infinity;
            
            _feedPage.TabSelectionStarted += OnTabSelectionStarted;
            _pageManager.PageSwitchingBegan += OnPageSwitchingBehan;
            _feedManagerView.OnFirstVideoLoadedEvent += OnFirstVideoLoaded;

            _appEventsSource.ApplicationQuit += OnApplicationQuit;
        }

        public void Dispose()
        {
            _feedPage.TabSelectionStarted -= OnTabSelectionStarted;
            _pageManager.PageSwitchingBegan -= OnPageSwitchingBehan;
            _feedManagerView.OnFirstVideoLoadedEvent -= OnFirstVideoLoaded;
            
            _appEventsSource.ApplicationQuit -= OnApplicationQuit;
        }

        public void Tick()
        {
            if (!IsFeed || _nextSendTime > Time.time) return;
            
            FireSendEvent(VideoViewsSendEventReason.TimeReached);
        }

        private void OnApplicationQuit()
        {
            if (!IsFeed) return;
            
            FireSendEvent(VideoViewsSendEventReason.UnsentEvents);
        }
        
        private void OnFirstVideoLoaded() => _nextSendTime = Time.time + SEND_RATE;

        private void OnPageSwitchingBehan(PageId? previousPageId, PageData nextPageData)
        {
            if (previousPageId is not (PageId.Feed or PageId.GamifiedFeed)) return;
            
            FireSendEvent(VideoViewsSendEventReason.PageChanged);
        }

        private void OnTabSelectionStarted(VideoListType videoListType)
        {
            FireSendEvent(VideoViewsSendEventReason.TabChanged);
        }

        private void FireSendEvent(VideoViewsSendEventReason reason)
        {
            _nextSendTime = Time.time + SEND_RATE;
            var sendEvent = new VideoViewsSendEvent(reason, _feedPage.OpenPageArgs.Name, _feedPage.CurrentVideoType);
            
            SendTriggered?.Invoke(sendEvent);
        }
    }
}
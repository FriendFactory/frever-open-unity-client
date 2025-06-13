using System.Linq;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Feed.Core.VideoViewTracking;
using UnityEngine;
using Zenject;

namespace UIManaging.Common
{
    public sealed class NavBarFeedButton : NavBarButtonBase
    {
        [Inject] private VideoManager _videoManager;
        [Inject] private PageManager _pageManager;
        [Inject] private VideoViewTracker _videoViewTracker;

        protected override void OnButtonClicked() 
        {
            if (_pageManager.IsChangingPage) return;

            if (_pageManager.CurrentPage.Id == PageId.Feed)
            {
                RefreshFeed();
                return;
            }
            
            if (ShouldOpenOnLastOpenedState())
            {
                OpenOnLastState();
            }
            else
            {
                _pageManager.MoveNext(PageId.Feed, new GeneralFeedArgs(_videoManager));
            }
        }

        private void RefreshFeed()
        {
            var pageArgs = (_pageManager.CurrentPage as GenericPage<BaseFeedArgs>).OpenPageArgs;
            var lastViewId = _videoViewTracker.GetLastSeenVideoId(pageArgs.VideoListType, true);
            _videoManager.GetFeedVideos(videoType: pageArgs.VideoListType, videoId: lastViewId, takeNextCount:2, onSuccess:
                                        videos =>
                                        { 
                                            long? nextVideo = videos.Any() ? videos.Last().Id : null;
                                            OpenFeed(nextVideo);
                                        }, onFail: error =>
                                        { 
                                            Debug.LogError($"Failed to get next videos: {pageArgs.VideoListType}");
                                            OpenFeed(null);
                                        }, takePreviousCount:0, cancellationToken: default );
            return;

            void OpenFeed(long? initialVideoId)
            {
                var args = new GeneralFeedArgs(_videoManager);
                args.SetIdOfFirstVideoToShow(initialVideoId);
                args.VideoListType = pageArgs.VideoListType;
                _pageManager.MoveNext(args);
            }
        }

        private bool ShouldOpenOnLastOpenedState()
        {
            var enteredFeedBefore = _pageManager.HistoryContains(PageId.Feed);
            if (!enteredFeedBefore) return false;

            var wasItGeneralFeed = _pageManager.GetLastArgsForPage(PageId.Feed) is GeneralFeedArgs a;
            return wasItGeneralFeed;
        }

        private void OpenOnLastState()
        {
            _pageManager.MoveBackTo(PageId.Feed);
        }
    }
}
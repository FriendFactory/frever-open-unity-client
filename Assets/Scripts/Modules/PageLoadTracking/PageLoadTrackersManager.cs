using System.Linq;
using Navigation.Core;
using Zenject;

namespace Modules.PageLoadTracking
{
    public sealed class PageLoadTrackersManager : IInitializable
    {
        private readonly IPageLoadTimeTracker[] _pageLoadTimeTrackers;
        private readonly PageManager _pageManager;

        public PageLoadTrackersManager(IPageLoadTimeTracker[] pageLoadTimeTrackers, PageManager pageManager)
        {
            _pageLoadTimeTrackers = pageLoadTimeTrackers;
            _pageManager = pageManager;
        }

        public void Initialize()
        {
            _pageManager.PageSwitchingBegan += OnPageSwitchingBegan;
        }

        private void OnPageSwitchingBegan(PageId? pageId, PageData nextPageData)
        {
            var tracker = nextPageData.PageId switch
            {
                PageId.LevelEditor => _pageLoadTimeTrackers.First(x => x.Type == LoadTimeTrackerType.LevelEditor),
                PageId.UmaEditor => _pageLoadTimeTrackers.First(x => x.Type == LoadTimeTrackerType.UmaEditor),
                _ => _pageLoadTimeTrackers.First(x => x.Type == LoadTimeTrackerType.Default)
            };

            tracker.BeginTracking(nextPageData);
        }
    }
}
using JetBrains.Annotations;
using Navigation.Core;

namespace Modules.PageLoadTracking
{
    [UsedImplicitly]
    internal sealed class PageLoadTimeTracker: PageLoadTimeTrackerBase<PageArgs>
    {
        public override LoadTimeTrackerType Type => LoadTimeTrackerType.Default;
        
        private readonly PageManager _pageManager;
        
        public PageLoadTimeTracker(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        protected override void OnTrackingStarted(PageArgs pageArgs)
        {
            _pageManager.PageDisplayed += OnPageDisplayed;

            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                
                OnTrackingEnded(pageData.PageArgs);
            }
        }
    }
}
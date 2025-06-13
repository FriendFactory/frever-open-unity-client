using System;
using System.Diagnostics;
using Navigation.Core;

namespace Modules.PageLoadTracking
{
    internal abstract class PageLoadTimeTrackerBase<TPageArgs> : IPageLoadTimeTracker where TPageArgs: PageArgs
    {
        private readonly Stopwatch _stopwatch = new ();

        public abstract LoadTimeTrackerType Type { get; }
        public event Action<PageArgs> TrackingStarted;
        public event Action<PageArgs, long> TrackingEnded;
        public long ElapsedMs => _stopwatch.ElapsedMilliseconds;
        
        public bool IsPageLoading { get; private set; }

        public void BeginTracking(PageData nextPageData)
        {
            if (nextPageData.PageArgs is not TPageArgs pageArgs) return;
            
            _stopwatch.Restart();
            IsPageLoading = true;
            TrackingStarted?.Invoke(pageArgs);
            OnTrackingStarted(pageArgs);
        }

        protected abstract void OnTrackingStarted(TPageArgs pageArgs);

        protected void OnTrackingEnded(PageArgs pageArgs)
        {
            _stopwatch.Stop();
            IsPageLoading = false;
            TrackingEnded?.Invoke(pageArgs, _stopwatch.ElapsedMilliseconds);
        }
    }
}
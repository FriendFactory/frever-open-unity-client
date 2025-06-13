using System;
using Navigation.Core;

namespace Modules.PageLoadTracking
{
    public enum LoadTimeTrackerType
    {
        Default = 0,
        LevelEditor = 1,
        UmaEditor = 2
    }
    
    public interface IPageLoadTimeTracker
    {
        LoadTimeTrackerType Type { get; }
        
        event Action<PageArgs> TrackingStarted;
        event Action<PageArgs, long> TrackingEnded;

        long ElapsedMs { get; }
        
        bool IsPageLoading { get; }
        
        void BeginTracking(PageData nextPageData);
    }
}
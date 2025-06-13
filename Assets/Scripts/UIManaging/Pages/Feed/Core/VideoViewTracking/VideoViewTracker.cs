using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.VideoServer;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.Feed.Core.VideoViewTracking
{
    [UsedImplicitly]
    public sealed class VideoViewTracker
    {
        private readonly Dictionary<VideoListType, List<VideoView>> _videoTrackers = new Dictionary<VideoListType, List<VideoView>>();

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action AddedVideoView; 
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VideoViewTracker()
        {
            var feedTypes = Enum.GetValues(typeof(VideoListType));
            
            foreach (VideoListType feedType in feedTypes)
            {
                _videoTrackers[feedType] = new List<VideoView>();
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Add(long videoId, VideoListType feedTab, string feedType)
        {
            var videoHashSet = GetVideoList(feedTab);
            
            var videoView = new VideoView
            {
                VideoId = videoId,
                ViewDate = DateTime.UtcNow,
                FeedTab = feedTab.ToString(),
                FeedType = feedType,
            };
            
            videoHashSet.Add(videoView);            
            AddedVideoView?.Invoke();
        }

        public long GetLastSeenVideoId(VideoListType feedType, bool ignoreRepeatableViews)
        {
            var videoList = GetVideoList(feedType);
            if (ignoreRepeatableViews)
            {
                return videoList.DistinctBy(x => x.VideoId).Last().VideoId;
            }
            return videoList.Last().VideoId;
        }

        public VideoView[] GetVideosViewedAfter(DateTime date)
        {
            return _videoTrackers.Values
                                 .SelectMany(x => x)
                                 .Where(x => x.ViewDate > date)
                                 .ToArray();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private List<VideoView> GetVideoList(VideoListType type)
        {
            return _videoTrackers[type];
        }
    }
}

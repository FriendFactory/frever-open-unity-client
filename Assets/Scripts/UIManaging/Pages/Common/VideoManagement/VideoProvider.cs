using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.VideoServer;
using Bridge.Services.UserProfile;
using Bridge.VideoServer.Models;
using Common.Services;
using JetBrains.Annotations;
using UIManaging.Pages.Common.FollowersManagement;

namespace UIManaging.Pages.Common.VideoManagement
{
    /// <summary>
    /// Provides video for new, following and featured feeds
    /// Uses cache for video where it's necessary and possible
    /// </summary>
    [UsedImplicitly]
    internal sealed class VideoProvider
    {
        private static readonly VideoListType[] ALL_FEED_TYPES =
            Enum.GetValues(typeof(VideoListType)).Cast<VideoListType>().ToArray();
        
        private readonly IVideoLoader[] _loaders;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public VideoProvider(FollowersManager followersManager, ITemplateManagingService templateManagingService, IVideoLoader[] videoLoaders)
        {
            _loaders = videoLoaders;
            followersManager.LocalUserFollowed.OnChanged += ClearFollowingAndFriendsFeedCaches;
            followersManager.LocalUserFollower.OnChanged += ClearFollowingAndFriendsFeedCaches;
            templateManagingService.TemplateRenamed += UpdateTemplateName;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public Task<FeedVideoResponse> GetVideosAsync(VideoLoadArgs args)
        {
            var loader = GetVideoLoader(args.VideoType);
            if (loader == null)
            {
                throw new InvalidOperationException($"There is no registered loader for video {args.VideoType}");
            }

            return loader.GetVideoAsync(args);
        }

        public void ClearCache(params VideoListType[] feedTypes)
        {
            if (feedTypes.Length == 0)
            {
                feedTypes = ALL_FEED_TYPES;
            }

            foreach (var feedType in feedTypes)
            {
                var loader = GetVideoLoader(feedType);
                if (loader != null && loader.SupportCache)
                {
                    loader.ClearCache();
                }
            }
        }

        public void UpdateVideoKpi(long videoId, VideoKPI kpi)
        {
            foreach (var feedType in ALL_FEED_TYPES)
            {
                var loader = GetVideoLoader(feedType);
                if (loader != null && loader.SupportCache)
                {
                    loader.UpdateVideoKPIInCache(videoId, kpi);
                }
            } 
        }
        
        public void UpdateVideoPrivacyInCache(long videoId, VideoAccess access)
        {
            foreach (var feedType in ALL_FEED_TYPES)
            {
                var loader = GetVideoLoader(feedType);
                if (loader != null && loader.SupportCache)
                {
                    loader.UpdateVideoPrivacyInCache(videoId, access);
                }
            } 
        }
        
        public void RemoveVideoInCache(long videoId)
        {
            foreach (var feedType in ALL_FEED_TYPES)
            {
                var loader = GetVideoLoader(feedType);
                if (loader != null && loader.SupportCache)
                {
                    loader.RemoveVideoInCache(videoId);
                }
            } 
        }

        public void ClearAllVideosBefore(VideoListType target, long videoId, bool includingTarget = true)
        {
            var loader = GetVideoLoader(target);
            if (loader != null && loader.SupportCache)
            {
                loader.ClearAllBefore(videoId, includingTarget);
            }
        }

        public void AddLikeToVideoInCache(long videoId)
        {
            foreach (var feedType in ALL_FEED_TYPES)
            {
                var loader = GetVideoLoader(feedType);
                if (loader == null || !loader.SupportCache) continue;
                loader.AddLikeToVideoInCache(videoId);
            } 
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IVideoLoader GetVideoLoader(VideoListType videoType)
        {
            return _loaders.FirstOrDefault(x => x.VideoType == videoType);
        }
        
        private void ClearFollowingAndFriendsFeedCaches(List<Profile> obj)
        {
            ClearCache(VideoListType.Following, VideoListType.Friends);
        }

        private void UpdateTemplateName(string previousName, string newName)
        {
            var cacheSupportedLoaders = _loaders.Where(x => x is IVideoListCacheControl).Cast<IVideoListCacheControl>();
            foreach (var cache in cacheSupportedLoaders)
            {
                cache.UpdateTemplateName(previousName, newName);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.VideoServer.Models;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    internal abstract class MultipleListCacheVideoLoader<TArgs, TKey> : CachedVideoLoader<TArgs>
        where TArgs: VideoLoadArgs
    {
        private readonly IBlockedAccountsManager _blockedAccountsManager;
        protected readonly Dictionary<TKey, VideoCache> Caches = new Dictionary<TKey, VideoCache>();

        protected MultipleListCacheVideoLoader(IVideoBridge bridge, IBlockedAccountsManager blockedAccountsManager) : base(bridge)
        {
            _blockedAccountsManager = blockedAccountsManager;
        }

        public sealed override void ClearCache()
        {
            foreach (var cache in Caches.Values)
            {
                cache.Clear();
            }
            Caches.Clear();
        }

        public sealed override void UpdateVideoKPIInCache(long videoId, VideoKPI kpi)
        {
            foreach (var cache in Caches.Values)
            {
                var video = cache.GetVideo(videoId);
                if (video == null) continue;
                video.KPI = kpi;
            }
        }

        public override void UpdateVideoPrivacyInCache(long videoId, VideoAccess access)
        {
            foreach (var cache in Caches.Values)
            {
                var video = cache.GetVideo(videoId);
                if (video == null) continue;
                video.Access = access;
            }
        }

        public sealed override void RemoveVideoInCache(long videoId)
        {
            foreach (var cache in Caches.Values)
            {
                cache.RemoveVideo(videoId);
            }
        }

        public override void UpdateTemplateName(string previousName, string newName)
        {
            foreach (var cache in Caches.Values)
            {
                cache.UpdateTemplateName(previousName, newName);
            }
        }
        
        public override void ClearAllBefore(long videoId, bool includingTarget = true)
        {
            foreach (var cache in Caches.Values)
            {
                cache.ClearVideosBefore(videoId, includingTarget);
            }
        }

        public override void AddLikeToVideoInCache(long videoId)
        {
            foreach (var cache in Caches.Values)
            {
                var video = cache.GetVideo(videoId);
                if (video == null) return;
                video.KPI.Likes++;
            }
        }

        protected sealed override VideoCache GetCache(TArgs args)
        {
            var key = GetCacheKey(args);
            return GetCache(key);
        }

        protected VideoCache GetCache(TKey key)
        {
            if (!Caches.ContainsKey(key))
            {
                Caches.Add(key, new VideoCache(_blockedAccountsManager));
            }
            return Caches[key];
        }

        protected abstract TKey GetCacheKey(TArgs args);
    }
}
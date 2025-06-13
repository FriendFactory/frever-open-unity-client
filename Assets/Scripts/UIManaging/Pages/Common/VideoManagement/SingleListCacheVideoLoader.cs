using Bridge;
using Bridge.Models.VideoServer;
using Bridge.VideoServer.Models;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    /// <summary>
    /// Has only 1 list cache, because request don't have sub categories parameters or special filters, such as
    /// target user id, hashtag id etc
    /// </summary>
    internal abstract class SingleListCacheVideoLoader<T>: CachedVideoLoader<T>
        where T : VideoLoadArgs
    {
        protected readonly VideoCache Cache;

        protected SingleListCacheVideoLoader(IBlockedAccountsManager blockedUsersManager, IVideoBridge bridge) : base(bridge)
        {
            Cache = new VideoCache(blockedUsersManager);
        }

        public override void ClearCache()
        {
            Cache.Clear();
        }

        public override void UpdateVideoKPIInCache(long videoId, VideoKPI kpi)
        {
            var video = Cache.GetVideo(videoId);
            if (video != null)
            {
                video.KPI = kpi;
            }
        }

        public override void UpdateVideoPrivacyInCache(long videoId, VideoAccess access)
        {
            var video = Cache.GetVideo(videoId);
            if (video == null) return;
            video.Access = access;
        }

        public sealed override void RemoveVideoInCache(long videoId)
        {
            Cache.RemoveVideo(videoId);
        }

        public override void UpdateTemplateName(string previousName, string newName)
        {
            Cache.UpdateTemplateName(previousName, newName);
        }

        public override void ClearAllBefore(long videoId, bool includingTarget = true)
        {
            Cache.ClearVideosBefore(videoId, includingTarget);
        }

        public override void AddLikeToVideoInCache(long videoId)
        {
            var video = Cache.GetVideo(videoId);
            if (video == null) return;
            video.KPI.Likes++;
        }

        protected sealed override VideoCache GetCache(T args)
        {
            return Cache;
        }
    }
}
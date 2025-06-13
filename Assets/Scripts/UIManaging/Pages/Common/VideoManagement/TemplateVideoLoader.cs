using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    public sealed class TemplateVideoLoadArgs : VideoLoadArgs
    {
        public long TemplateId;
        public override VideoListType VideoType => VideoListType.Template;
    }
    
    [UsedImplicitly]
    internal sealed class TemplateVideoLoader: MultipleListCacheVideoLoader<TemplateVideoLoadArgs, long>, IVideoPrivacyChangingListener
    {
        public override VideoListType VideoType => VideoListType.Template;
        
        public TemplateVideoLoader(IVideoBridge bridge, IBlockedAccountsManager blockedAccountsManager) 
            : base(bridge, blockedAccountsManager)
        {
        }
        
        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(TemplateVideoLoadArgs args)
        {
            return Bridge.GetVideoForTemplate(args.TemplateId, args.TargetVideoKey, args.TakeNext,
                                              args.CancellationToken);
        }

        protected override long GetCacheKey(TemplateVideoLoadArgs args)
        {
            return args.TemplateId;
        }

        public void OnVideoPrivacyChanged(Video video)
        {
            if (video.TemplateIds.IsNullOrEmpty()) return;
            
            if (video.Access == VideoAccess.Private)
            {
                RemoveFromCachedLists(video);
            }
            else
            {
                ClearCache();
            }
        }

        private void RemoveFromCachedLists(Video video)
        {
            foreach (var cache in Caches.Values)
            {
                if (!cache.HasInCache(video.Id)) continue;
                cache.RemoveVideo(video.Id);
            }
        }
    }
}
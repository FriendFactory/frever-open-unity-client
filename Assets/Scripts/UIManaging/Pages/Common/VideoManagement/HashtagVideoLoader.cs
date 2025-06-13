using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    public sealed class HashtagVideoLoadArgs: VideoLoadArgs
    {
        public long HashtagId;

        public override VideoListType VideoType => VideoListType.Hashtag;
    }
    
    [UsedImplicitly]
    internal sealed class HashtagVideoLoader: MultipleListCacheVideoLoader<HashtagVideoLoadArgs, long>
    {
        public override VideoListType VideoType => VideoListType.Hashtag;

        public HashtagVideoLoader(IVideoBridge bridge, IBlockedAccountsManager blockedAccountsManager) 
            : base(bridge, blockedAccountsManager)
        {
        }

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(HashtagVideoLoadArgs args)
        {
            return Bridge.GetHashtagVideoListAsync(args.HashtagId, args.TargetVideoKey, args.TakeNext,
                                                   args.CancellationToken);
        }

        protected override long GetCacheKey(HashtagVideoLoadArgs args)
        {
            return args.HashtagId;
        }
    }
}
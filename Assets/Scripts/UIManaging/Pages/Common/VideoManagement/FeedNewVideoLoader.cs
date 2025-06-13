using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    internal sealed class FeedNewVideoLoader : SingleListCacheVideoLoader<VideoLoadArgs>
    {
        public override VideoListType VideoType => VideoListType.New;

        public FeedNewVideoLoader(IBlockedAccountsManager blockedAccountsManager, IVideoBridge bridge) : base(blockedAccountsManager, bridge)
        {
        }

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(VideoLoadArgs args)
        {
            return Bridge.GetFeedVideoListAsync(args.TargetVideoKey, args.TakeNext, args.CancellationToken);
        }
    }
}
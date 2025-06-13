using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    internal sealed class FollowingFeedVideoLoader : SingleListCacheVideoLoader<VideoLoadArgs>
    {
        public override VideoListType VideoType => VideoListType.Following;

        public FollowingFeedVideoLoader(IBlockedAccountsManager blockedAccountsManager, IVideoBridge bridge) : base(blockedAccountsManager, bridge)
        {
        }

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(VideoLoadArgs args)
        {
            return Bridge.GetMyFollowingVideoListAsync(args.TargetVideoKey, args.TakeNext, args.CancellationToken);
        }
    }
}
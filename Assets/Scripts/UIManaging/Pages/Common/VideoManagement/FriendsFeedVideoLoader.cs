using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    internal sealed class FriendsFeedVideoLoader : SingleListCacheVideoLoader<VideoLoadArgs>
    {
        public override VideoListType VideoType => VideoListType.Friends;

        public FriendsFeedVideoLoader(IBlockedAccountsManager blockedUsersManager, IVideoBridge bridge) : base(blockedUsersManager, bridge)
        {
        }

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(VideoLoadArgs args)
        {
            return Bridge.GetMyFriendsVideoListAsync(args.TargetVideoKey, args.TakeNext, args.CancellationToken);
        }
    }
}
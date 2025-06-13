using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    public sealed class TaskVideoLoaderArgs : VideoLoadArgs
    {
        public long TaskId;
        public override VideoListType VideoType => VideoListType.Task;
    }

    [UsedImplicitly]
    internal sealed class TaskVideoLoader : MultipleListCacheVideoLoader<TaskVideoLoaderArgs, long>
    {
        public TaskVideoLoader(IVideoBridge bridge, IBlockedAccountsManager blockedAccountsManager)
            : base(bridge, blockedAccountsManager)
        {
        }

        public override VideoListType VideoType => VideoListType.Task;

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(TaskVideoLoaderArgs args)
        {
            return Bridge.GetTaskVideoListAsync(args.TaskId, args.TargetVideoKey, args.TakeNext,
                                                args.CancellationToken);
        }

        protected override long GetCacheKey(TaskVideoLoaderArgs args)
        {
            return args.TaskId;
        }
    }
}
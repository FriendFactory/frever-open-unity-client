using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    public sealed class UserTaskVideoLoaderArgs : VideoLoadArgs
    {
        public long UserGroupId { get; set; }
        public override VideoListType VideoType => VideoListType.UserTask;
    }

    [UsedImplicitly]
    internal sealed class UserTaskVideoLoader : MultipleListCacheVideoLoader<UserTaskVideoLoaderArgs, long>, IVideoUploadingListener
    {
        private readonly LocalUserDataHolder _localUserDataHolder;
        public override VideoListType VideoType => VideoListType.UserTask;

        public UserTaskVideoLoader(IVideoBridge bridge, IBlockedAccountsManager blockedAccountsManager, LocalUserDataHolder localUserDataHolder) : base(bridge, blockedAccountsManager)
        {
            _localUserDataHolder = localUserDataHolder;
        }

        public void OnVideoUploaded(Video video)
        {
            if (video.TaskId <= 0) return;

            video.Key = video.Id.ToString();
            var localUserCache = GetCache(_localUserDataHolder.GroupId);
            var videosPostedLater = localUserCache.AvailableVideos.TakeWhile(x => x.Id > video.Id);
            var indexToInsert = videosPostedLater.Count();
            localUserCache.Insert(indexToInsert, video);
        }

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(UserTaskVideoLoaderArgs args)
        {
            return Bridge.GetUserTasksVideoListAsync(args.UserGroupId, args.TargetVideoKey, args.TakeNext, args.TakePrevious,
                                              args.CancellationToken);
        }

        protected override long GetCacheKey(UserTaskVideoLoaderArgs args)
        {
            return args.UserGroupId;
        }
    }
}
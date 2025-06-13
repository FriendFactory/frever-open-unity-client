using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Bridge.Results;
using JetBrains.Annotations;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;

namespace UIManaging.Pages.Common.VideoManagement
{
    [UsedImplicitly]
    internal sealed class ProfileVideoLoader : SingleListCacheVideoLoader<VideoLoadArgs>, IVideoUploadingListener
    {
        public override VideoListType VideoType => VideoListType.Profile;

        public ProfileVideoLoader(IBlockedAccountsManager blockedUsersManager, IVideoBridge bridge) : base(blockedUsersManager, bridge)
        {
        }

        public void OnVideoUploaded(Video video)
        {
            if (video.TaskId > 0 || video.IsPublishedAsMessage()) return;
            var videosPostedLater = Cache.AvailableVideos.TakeWhile(x => x.Id > video.Id);
            var indexToInsert = videosPostedLater.Count();
            Cache.Insert(indexToInsert, video);
        }

        protected override Task<EntitiesResult<Video>> DownloadFromServerAsync(VideoLoadArgs args)
        {
            return Bridge.GetMyVideoListAsync(args.TargetVideoId, args.TakeNext, args.TakePrevious, args.CancellationToken);
        }
    }
}

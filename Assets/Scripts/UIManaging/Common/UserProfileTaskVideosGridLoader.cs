using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LevelPreviews;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Common
{
    public sealed class UserProfileTaskVideosGridLoader : LocalUserVideoListLoaderBase
    {
        public UserProfileTaskVideosGridLoader(VideoManager videoManager, PageManager pageManager, IBridge bridge, long userGroupId) : base(videoManager, pageManager, bridge, userGroupId)
        {
        }

        protected override Task<Video[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            return VideoManager.GetUserVideoForTasks(targetId as long?, UserGroupId, takeNext, takePrevious, token);
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new UserTaskFeedArgs(args.Video.GroupId, VideoManager, args.Video.Id));
        }

        protected override BaseLevelItemArgs VideoToLevelPreviewArgs(Video video)
        {
            return new LevelPreviewItemArgs(video, OnVideoPreviewClicked, showScore:true, showLikes:false);
        }
    }
}
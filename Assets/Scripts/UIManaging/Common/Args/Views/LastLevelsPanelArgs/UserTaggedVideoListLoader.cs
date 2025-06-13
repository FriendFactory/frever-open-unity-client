using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.VideoServer;
using Bridge;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Common.Args.Views.LastLevelsPanelArgs
{
    public class UserTaggedVideoListLoader : BaseVideoListLoader
    {
        public UserTaggedVideoListLoader(VideoManager videoManager, PageManager pageManager, IBridge bridge, long groupId) 
            : base(videoManager, pageManager, bridge, groupId)
        { }

        protected override async Task<Video[]> DownloadModelsInternal(object targetVideo, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            return (await Bridge.GetUserTaggedVideoListAsync(UserGroupId, (long?)targetVideo, takeNext, takePrevious, token)).Models;
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new TaggedUserFeedArgs(VideoManager, UserGroupId, args.Video.Id));
        }
    }
}
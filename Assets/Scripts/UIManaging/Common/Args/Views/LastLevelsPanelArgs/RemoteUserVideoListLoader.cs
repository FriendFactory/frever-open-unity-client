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
    public class RemoteUserVideoListLoader : BaseVideoListLoader
    {
        public RemoteUserVideoListLoader(VideoManager videoManager, PageManager pageManager, IBridge bridge, long userGroupId) 
            : base(videoManager, pageManager, bridge, userGroupId)
        { }

        protected override async Task<Video[]> DownloadModelsInternal(object targetVideo, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            return (await Bridge.GetPublicVideoForAccount(UserGroupId, (long?)targetVideo, takeNext, cancellationToken: token)).Models;
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new RemoteUserFeedArgs(VideoManager, UserGroupId, args.Video.Id));
        }
    }
}
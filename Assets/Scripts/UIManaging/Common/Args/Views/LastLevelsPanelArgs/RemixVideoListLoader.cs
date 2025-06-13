using System.Threading;
using Bridge.Models.VideoServer;
using Bridge;
using UIManaging.Pages.Common.VideoManagement;
using System.Threading.Tasks;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;

namespace UIManaging.Common.Args.Views.LastLevelsPanelArgs
{
    public class RemixVideoListLoader : BaseVideoListLoader
    {
        private readonly long _videoId;
        private readonly long? _remixedFromVideoId;

        public RemixVideoListLoader(long videoId, long? remixedFromVideoId, VideoManager videoManager, PageManager pageManager, IBridge bridge)
            : base(videoManager, pageManager, bridge, bridge.Profile.GroupId)
        {
            _videoId = videoId;
            _remixedFromVideoId = remixedFromVideoId;
        }

        protected override async Task<Video[]> DownloadModelsInternal(object targetVideo, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            return (await Bridge.GetRemixVideoListAsync(_videoId, (long?)targetVideo, takeNext, takePrevious, token)).Models;
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new RemixFeedArgs(_remixedFromVideoId, _videoId, Bridge, VideoManager, args.Video.Id));
        }
    }
}
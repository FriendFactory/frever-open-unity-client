using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;

namespace UIManaging.Pages.VideosBasedOnTemplatePage
{
    public class HashtagTemplateVideoListLoader : BaseVideoListLoader
    {
        private readonly HashtagInfo _hashtagInfo;
        
        public HashtagTemplateVideoListLoader(HashtagInfo hashtagInfo, VideoManager videoManager, PageManager pageManager, IBridge bridge, long groupId) 
            : base(videoManager, pageManager, bridge, groupId)
        {
            _hashtagInfo = hashtagInfo;
        }

        protected override async Task<Video[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await VideoManager.GetHashtagVideos(_hashtagInfo.Id, (long?)targetId, takeNext, takePrevious, token);
            return result.Video;
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new HashtagFeedArgs(_hashtagInfo, VideoManager, args.Video.Id, Models.IndexOf(args.Video)));
        }
    }
}
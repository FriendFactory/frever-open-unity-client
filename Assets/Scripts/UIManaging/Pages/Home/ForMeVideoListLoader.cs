using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    public class ForMeVideoListLoader : BaseVideoListLoader
    {
        protected override int DefaultPageSize => 5;
        
        //---------------------------------------------------------------------
        // ctors
        //---------------------------------------------------------------------
        
        public ForMeVideoListLoader(VideoManager videoManager, PageManager pageManager, IBridge bridge, long userGroupId)
            : base(videoManager, pageManager, bridge, userGroupId) { }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new GeneralFeedArgs(VideoManager, args.Video.Id));
        }

        protected override Task<Video[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            return VideoManager.GetFeedVideosAsync(OnFail, VideoListType.ForMe, (long?)targetId, takeNext, takePrevious, token);

            void OnFail(string message)
            {
                Debug.LogError($"Failed to load for me feed, reason: {message}");
            }
        }
    }
}
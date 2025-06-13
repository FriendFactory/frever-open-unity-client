using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine.Events;

namespace UIManaging.Pages.VideosBasedOnTemplatePage
{
    public class TemplateVideoListLoader : BaseVideoListLoader
    {
        private event Action OnJoinTemplateClick;
        protected readonly long TemplateId;

        public TemplateVideoListLoader(long templateId, VideoManager videoManager, PageManager pageManager,
            IBridge bridge, long groupId, UnityAction onJoinTemplateRequested) 
            : base(videoManager, pageManager, bridge, groupId)
        {
            TemplateId = templateId;
        }

        protected override async Task<Video[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await VideoManager.GetVideoForTemplate(TemplateId, (long?)targetId, takeNext, takePrevious, token);
            return result.Video;
        }
        
        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new VideosBasedOnTemplateFeedArgs(VideoManager, args.Video.Id, Models.IndexOf(args.Video), TemplateId, OnJoinTemplateClick));
        }
    }
}
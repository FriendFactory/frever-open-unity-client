using Bridge.Models.VideoServer;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.VideosBasedOnTemplatePage;

namespace Navigation.Args
{
    public class VideosBasedOnHashtagPageArgs : BaseVideoTemplatePageArgs
    {
        public HashtagInfo HashtagInfo;

        public override TemplateType TemplateType => TemplateType.Hashtag;

        public override BaseVideoListLoader GetVideoListLoader(PageManager pageManager, VideoManager videoManager)
        {
            return new HashtagTemplateVideoListLoader(HashtagInfo, videoManager, pageManager, null, 0);
        }
    }
}

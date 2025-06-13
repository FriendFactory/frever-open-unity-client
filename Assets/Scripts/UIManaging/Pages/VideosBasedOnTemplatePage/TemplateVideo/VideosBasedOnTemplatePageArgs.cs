using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.VideosBasedOnTemplatePage;

namespace Navigation.Args
{
    public class VideosBasedOnTemplatePageArgs : BaseVideoTemplatePageArgs
    {
        public override TemplateType TemplateType => TemplateType.Challenge;
        
        public override BaseVideoListLoader GetVideoListLoader(PageManager pageManager, VideoManager videoManager)
        {
            return new TemplateVideoListLoader(TemplateInfo.Id, videoManager, pageManager, null, 0, OnJoinTemplateRequested);
        }
    }
}

using Bridge.Models.VideoServer;
using Navigation.Core;

namespace UIManaging.Pages.PreRemixPage.Ui
{
    public class PreRemixPageArgs : PageArgs
    {
        public override PageId TargetPage { get; } = PageId.PreRemixPage;
        public Video Video { get; }

        public PreRemixPageArgs(Video video)
        {
            Video = video;
        }
    }
}


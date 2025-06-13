using Bridge.Models.VideoServer;
using Navigation.Core;

namespace UIManaging.Pages.ReportPage.Ui.Args
{
    public sealed class ReportPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.ReportPage;

        public readonly Video Video;

        public ReportPageArgs(Video video)
        {
            Video = video;
        }
    }
}


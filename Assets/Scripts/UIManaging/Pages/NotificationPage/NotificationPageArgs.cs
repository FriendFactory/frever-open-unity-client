using Navigation.Core;

namespace UIManaging.Pages.NotificationPage
{
    public sealed class NotificationPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.NotificationPage;
        
        public bool ForceRefresh { get; set; }
    }
}

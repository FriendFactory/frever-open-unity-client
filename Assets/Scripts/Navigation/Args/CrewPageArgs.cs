using Navigation.Core;

namespace Navigation.Args
{
    public sealed class CrewPageArgs : PageArgs
    {
        public bool OpenJoinRequests;
        
        public override PageId TargetPage => PageId.CrewPage;
        public string SearchQuery { get; set; } 
    }
}
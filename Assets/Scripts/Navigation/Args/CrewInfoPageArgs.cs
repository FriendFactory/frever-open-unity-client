using Bridge.Models.ClientServer.Crews;
using Navigation.Core;

namespace Navigation.Args
{
    public class CrewInfoPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.CrewInfo;

        public CrewShortInfo CrewShortInfo { get; }
        
        public CrewInfoPageArgs(CrewShortInfo crewShortInfo)
        {
            CrewShortInfo = crewShortInfo;
        }
    }
}
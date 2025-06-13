using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class TaskDetailsPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.TaskDetails;

        public readonly TaskFullInfo TaskFullInfo;
        public bool Visited { get; set; }

        public TaskDetailsPageArgs(TaskFullInfo taskFullInfo)
        {
            TaskFullInfo = taskFullInfo;
        }
    }
}
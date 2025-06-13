using Navigation.Core;

namespace Navigation.Args
{
    public sealed class TaskVideosGridPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.TaskVideoGrid;

        public readonly long TaskId;

        public TaskVideosGridPageArgs(long taskId)
        {
            TaskId = taskId;
        }
    }
}
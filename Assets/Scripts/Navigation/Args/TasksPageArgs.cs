using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;

namespace Navigation.Args
{
    public class TasksPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.TasksPage;

        public TaskFullInfo OpenedWithTask;
        
        public int TabIndex { get; set; }

        public TasksPageArgs(TaskFullInfo openedWithTask = null, int tabIndex = 0)
        {
            TabIndex = tabIndex;
            OpenedWithTask = openedWithTask;
        }
    }
}
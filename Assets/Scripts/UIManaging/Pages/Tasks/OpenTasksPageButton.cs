using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;

namespace UIManaging.Pages.Tasks
{
    public class OpenTasksPageButton : ButtonBase
    {
        protected override void OnClick()
        {
            Manager.MoveNext(PageId.TasksPage, new TasksPageArgs());
        }
    }
}
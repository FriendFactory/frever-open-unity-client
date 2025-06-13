using Navigation.Args;
using Navigation.Core;

namespace UIManaging.Pages.Home
{
    public sealed class ResponsiveOpenTasksButton : ResponsiveHomePageButtonBase
    {
        protected override void OnClick()
        {
            Manager.MoveNext(PageId.TasksPage, new TasksPageArgs());
        }
    }
}
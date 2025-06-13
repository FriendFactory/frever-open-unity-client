using System;
using System.Collections.Generic;
using UIManaging.Pages.Tasks.TaskVideosGridPage;

namespace UIManaging.Pages.Tasks
{
    public interface ITaskListModel
    {
        IReadOnlyList<TaskModel> Tasks { get; }
        TimeSpan? TimeToNewTasks { get; }

        event Action TasksChanged;

        void RequestPage();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Common.Loaders;
using UIManaging.Pages.Tasks.TaskVideosGridPage;

namespace UIManaging.Pages.Tasks
{
    public abstract class TaskListModelBase : GenericPaginationLoader<TaskInfo>, ITaskListModel
    {
        public IReadOnlyList<TaskModel> Tasks => _tasks;
        public virtual TimeSpan? TimeToNewTasks => null;

        public event Action TasksChanged;
        
        protected readonly IBridge Bridge;

        protected override int DefaultPageSize { get; }

        private List<TaskModel> _tasks;

        //---------------------------------------------------------------------
        // ctors
        //---------------------------------------------------------------------
        
        protected TaskListModelBase(IBridge bridge, int defaultPageSize = 5)
        {
            Bridge = bridge;
            DefaultPageSize = defaultPageSize;

            LastPageLoaded += OnTasksUpdated;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void CleanUp()
        {
            LastPageLoaded -= OnTasksUpdated;
        }
        
        public void RequestPage()
        {
            if (AwaitingData)
            {
                return;
            }
            
            DownloadNextPage();
        }

        public void ReloadData()
        {
            Models.Clear();
            _tasks.Clear();
            DownloadNextPage();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnNextPageLoaded(TaskInfo[] page)
        {
            OnTasksUpdated();
        }

        protected override void OnFirstPageLoaded(TaskInfo[] page)
        {
            OnTasksUpdated();
        }
        
        protected virtual void OnTasksUpdated()
        {
            _tasks = Models.Select(task =>
            {
                var existingModel = Tasks?.FirstOrDefault(taskModel => taskModel.Task.Id == task.Id);
                return existingModel ?? CreateModel(task);
            }).ToList();

            TasksChanged?.Invoke();
        }

        protected abstract TaskModel CreateModel(TaskInfo task);
    }
}
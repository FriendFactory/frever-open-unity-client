using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Tasks.TaskVideosGridPage;
using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    public sealed class TaskListModel: TaskListModelBase
    {
        private readonly VideoManager _videoManager;
        private readonly PageManager _pageManager;

        public override TimeSpan? TimeToNewTasks => _moreTasksTime.HasValue ? _moreTasksTime.Value - DateTime.UtcNow : TimeSpan.Zero;

        private DateTime? _moreTasksTime;

        public TaskListModel(IBridge bridge, VideoManager videoManager, PageManager pageManager, int defaultPageSize = 5) : base(bridge, defaultPageSize)
        {
            _videoManager = videoManager;
            _pageManager = pageManager;
        }
        
        protected override async void OnTasksUpdated() 
        {
            if (_moreTasksTime == null)
            {
                await MoreTasksTimeRequest();
            }
            
            base.OnTasksUpdated();
        }

        protected override async Task<TaskInfo[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await Bridge.GetTasksAsync((long?)targetId, takeNext, takePrevious, null, TaskType.Voting, token);

            if (result.IsError)
            {
                Debug.LogError("Error loading task videos: " + result.ErrorMessage);
                return null;
            }

            if (result.IsSuccess)
            {
                return result.Models;
            }

            return null;
        }

        protected override TaskModel CreateModel(TaskInfo taskInfo)
        {
            return new TaskModel(_videoManager, _pageManager, Bridge, taskInfo);
        }

        private async Task MoreTasksTimeRequest()
        {
            var result = await Bridge.GetNextTaskReleaseDate();

            if (result.IsError)
            {
                Debug.LogError($"Failed to get the time until next style battles, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                _moreTasksTime = result.DateTime;
            }
        }
    }
}
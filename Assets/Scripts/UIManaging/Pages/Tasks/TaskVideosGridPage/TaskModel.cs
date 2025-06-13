using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;

namespace UIManaging.Pages.Tasks.TaskVideosGridPage
{
    public class TaskModel : BaseVideoListLoader
    {
        public TaskInfo Task { get; }
        
        protected override int DefaultPageSize => 9;
        
        private readonly bool _taskActive;
        private TaskFullInfo _taskFullInfo;

        private long TaskId => _taskFullInfo?.Id ?? Task.Id;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public TaskModel(VideoManager videoManager, PageManager pageManager, IBridge bridge, TaskFullInfo taskFullInfo) 
            : base(videoManager, pageManager, bridge, 0)
        {
            _taskFullInfo = taskFullInfo;
            _taskActive = _taskFullInfo.Deadline > DateTime.UtcNow &&
                          (_taskFullInfo.SoftCurrencyPayout > 0 || _taskFullInfo.XpPayout > 0);
        }

        public TaskModel(VideoManager videoManager, PageManager pageManager, IBridge bridge, TaskInfo taskInfo) 
            : base(videoManager, pageManager, bridge, 0)
        {
            Task = taskInfo;
            _taskActive = taskInfo.Deadline > DateTime.UtcNow &&
                          (taskInfo.SoftCurrencyPayout > 0 || taskInfo.XpPayout > 0);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public TaskVideosGridHeaderViewArgs CreateViewArgs(bool requireTaskInfoUpdate = false)
        {
            var headerArgs = new TaskDetailsHeaderArgs(_taskFullInfo, requireTaskInfoUpdate);
            return new TaskVideosGridHeaderViewArgs(headerArgs, _taskActive);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override async Task<Video[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await VideoManager.GetTaskGridVideo(TaskId, (long?)LastLoadedItemId, LastLoadedItemKey, PageSize, 0, token);
            return result.Video;
        }

        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            PageManager.MoveNext(PageId.Feed, new TaskFeedArgs(VideoManager, args.Video.Id, TaskId));
        }

        public override async void OnTaskClicked()
        {
            if (_taskFullInfo == null) 
            {
                var result = await Bridge.GetTaskFullInfoAsync(Task.Id);
            
                if (result.IsError)
                {
                    Debug.LogError(result.ErrorMessage);
                    return;
                }

                _taskFullInfo = result.Model;
            }

            if (_taskFullInfo.TaskType == TaskType.Voting)
            {
                PageManager.MoveNext(PageId.TaskDetails, new TaskDetailsPageArgs(_taskFullInfo));
            }
            else
            {
                PageManager.MoveNext(PageId.TaskVideoGrid, new TaskVideosGridPageArgs(_taskFullInfo.Id));
            }
        }
    }
}
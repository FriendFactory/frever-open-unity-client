using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Tasks.TaskVideosGridPage;
using UIManaging.Pages.VotingResult;
using UIManaging.PopupSystem;
using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    public sealed class InVotingTaskListModel: TaskListModelBase
    {
        private readonly IVotingBattleResultManager _votingBattleResultManager;
        private readonly VideoManager _videoManager;
        private readonly PageManager _pageManager;
        private readonly PopupManager _popupManager;
        
        public InVotingTaskListModel(IVotingBattleResultManager votingBattleResultManager, IBridge bridge, VideoManager videoManager, PageManager pageManager, PopupManager popupManager, int defaultPageSize = 5) : base(bridge, defaultPageSize)
        {
            _votingBattleResultManager = votingBattleResultManager;
            _videoManager = videoManager;
            _pageManager = pageManager;
            _popupManager = popupManager;
        }

        protected override async Task<TaskInfo[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var result = await Bridge.GetJoinedVotingTasks((long?)targetId, takeNext, takePrevious, token);
            
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

        protected override TaskModel CreateModel(TaskInfo task)
        {
            return new VotingTaskModel(_votingBattleResultManager, _videoManager, _pageManager, Bridge, task, _popupManager);
        }
    }
}
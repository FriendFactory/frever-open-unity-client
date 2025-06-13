using System;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.VotingResult;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.Pages.Tasks.TaskVideosGridPage
{
    public sealed class VotingTaskModel : TaskModel
    {
        private readonly IVotingBattleResultManager _votingBattleResultManager;
        private readonly PopupManager _popupManager;

        public VotingTaskModel(IVotingBattleResultManager votingBattleResultManager, VideoManager videoManager, PageManager pageManager, IBridge bridge, TaskInfo taskInfo, PopupManager popupManager) : base(videoManager, pageManager, bridge, taskInfo)
        {
            _votingBattleResultManager = votingBattleResultManager;
            _popupManager = popupManager;
        }

        public override void OnTaskClicked()
        {
            TryOpenBattleResults();
        }

        protected override void OnVideoPreviewClicked(BaseLevelItemArgs args)
        {
            TryOpenBattleResults();
        }

        private async void TryOpenBattleResults()
        {
            if (!Task.BattleResultReadyAt.HasValue)
            {
                Debug.LogError("No battle result readiness time in task");
                return;
            }
            
            if (Task.BattleResultReadyAt.Value > DateTime.UtcNow)
            {
                var config = new WaitVotingResultPopupConfiguration();
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(config.PopupType);
            }
            else
            {
                var result = await _votingBattleResultManager.GetVotingBattleResult(Task.Id);
                
                var args = new VotingResultPageArgs(Task.Id, Task.Name, result);
                PageManager.MoveNext(args);
            }
        }
    }
}
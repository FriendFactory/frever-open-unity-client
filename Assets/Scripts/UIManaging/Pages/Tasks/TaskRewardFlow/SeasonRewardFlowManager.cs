using System;
using Bridge.Models.ClientServer.Tasks;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Tasks.TaskRewardFlow
{
    public sealed class SeasonRewardFlowManager : MonoBehaviour
    {
        [SerializeField] private RewardFlowManager rewardFlowManager;

        [Inject] private LocalUserDataHolder _userData;
        [Inject] private PopupManager _popupManager;

        public Action<bool> FlowCompleted;

        public void Initialize()
        {
            rewardFlowManager.Initialize(_userData.UserBalance, _userData.LevelingProgress.Xp);
        }

        public void Run(TaskFullInfo taskInfo)
        {
            StartTaskRewardAnimation(taskInfo.XpPayout, taskInfo.SoftCurrencyPayout, 0);
        }

        public void Run(int xpPayout, int softPayout, int hardPayout)
        {
            StartTaskRewardAnimation(xpPayout, softPayout, hardPayout);
        }

        private void StartTaskRewardAnimation(int xpPayout, int softPayout, int hardPayout)
        {
            rewardFlowManager.StartAnimation(xpPayout, softPayout, hardPayout);
            rewardFlowManager.FlowCompleted = OnRewardFlowCompleted;
        }

        private void OnRewardFlowCompleted(RewardFlowResult result)
        {
            if (!gameObject.activeInHierarchy) return;

            if (!result.LeveledUp)
            {
                FlowCompleted?.Invoke(false);
                return;
            }

            var config = new LevelUpPopupConfiguration(result.OldLevel, result.NewLevel, OnLevelPopupClosed);
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnLevelPopupClosed(object result)
        {
            FlowCompleted?.Invoke((bool)result);
        }
    }
}
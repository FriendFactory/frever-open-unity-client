using System.Linq;
using Bridge.Models.ClientServer.Onboarding;
using Common;
using Common.UserBalance;
using Modules.AssetsStoraging.Core;
using Modules.QuestManaging;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Home;
using UIManaging.Pages.Tasks;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Quests.Models;
using UIManaging.PopupSystem.Popups.Quests.Views;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Quests
{
    internal sealed class QuestPopup: ConfigurableBasePopup<QuestPopupConfiguration>
    {
        [SerializeField] private QuestPanelView _questPanelView;
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private RewardFlowManager _rewardFlowManager;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _backOverlay;
        [SerializeField] private SlideInOutBehaviour _slideInOutBehaviour;
        [SerializeField] private GameObject _inputBlocker;

        [Inject] private IQuestManager _questManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private IDataFetcher _dataFetcher;
        
        private QuestPanelModel _questPanelModel;
        private StaticUserBalanceModel _userBalanceModel;
        
        protected override void OnConfigure(QuestPopupConfiguration configuration)
        {
            _questPanelModel?.Cleanup();

            if (_questPanelModel != null)
            {
                _questPanelModel.RewardClaimed -= OnRewardClaimed;
            }
            
            _questPanelModel = new QuestPanelModel(_questManager);
            _questPanelModel.RewardClaimed += OnRewardClaimed;
            _questPanelModel.GoToQuestClicked += OnGoToQuestClicked;
            
            _questPanelView.Initialize(_questPanelModel);
            
            _userBalanceModel?.CleanUp();
            _userBalanceModel = new StaticUserBalanceModel(_localUserDataHolder);
            
            _userBalanceView.Initialize(_userBalanceModel);
            
            _backButton.onClick.AddListener(Hide);
            _backOverlay.onClick.AddListener(Hide);
            _inputBlocker.SetActive(true);
            
            _slideInOutBehaviour.SlideIn(() => _inputBlocker.SetActive(false));
            
            if (!_questManager.IsUpdating)
            {
                _questManager.UpdateQuestData();
            }
        }

        public override void Hide()
        {
            Config.OnHidingBegin?.Invoke();
            _inputBlocker.SetActive(true);
            _slideInOutBehaviour.SlideOut(base.Hide);
        }
        
        protected override void OnHidden()
        {
            base.OnHidden();
            
            _backButton.onClick.RemoveListener(Hide);
            _backOverlay.onClick.RemoveListener(Hide);
            
            _questPanelView.CleanUp();
            
            _questPanelModel?.Cleanup();
            
            if (_questPanelModel != null)
            {
                _questPanelModel.RewardClaimed -= OnRewardClaimed;
                _questPanelModel.GoToQuestClicked -= OnGoToQuestClicked;
            }
            
            _questPanelModel = null;
            
            _userBalanceView.CleanUp();
            
            _userBalanceModel?.CleanUp();
            _userBalanceModel = null;
        }

        private void OnRewardClaimed(OnboardingReward reward)
        {
            _rewardFlowManager.Initialize(_localUserDataHolder.UserBalance, _localUserDataHolder.LevelingProgress.Xp);
            
            if (_questManager.IsComplete)
            {
                var config = new TaskCompletedPopupConfiguration(reward.SoftCurrency ?? 0, reward.HardCurrency ?? 0, 0, StartQuestCompleteAnimation);
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(config.PopupType);
                return;
            }

            var softCurrency = reward.SoftCurrency ?? 0;
            var hardCurrency = reward.HardCurrency ?? 0;
            
            _rewardFlowManager.FlowCompleted += OnRewardFlowCompleted;
            _rewardFlowManager.StartAnimation(0, softCurrency, hardCurrency);
        }

        private void OnRewardFlowCompleted(RewardFlowResult result)
        {
            _rewardFlowManager.FlowCompleted -= OnRewardFlowCompleted;
        }
        
        private void StartQuestCompleteAnimation(object result)
        {
            var reward = _questManager.UserQuestGroup.QuestGroup.Rewards.FirstOrDefault();

            if (reward == null)
            {
                Debug.LogError($"Couldn't find rewards for quest group {_questManager.UserQuestGroup.QuestGroup.Id}");
                return;
            }

            _rewardFlowManager.FlowCompleted += OnQuestCompleteAnimationFinished;
            _rewardFlowManager.StartAnimation(0, reward.SoftCurrency ?? 0, reward.HardCurrency ?? 0);
        }

        private void OnQuestCompleteAnimationFinished(RewardFlowResult result)
        {
            _rewardFlowManager.FlowCompleted -= OnQuestCompleteAnimationFinished;
            Hide();
        }

        private void OnGoToQuestClicked(string questType)
        {
            Hide();
        }
    }
}
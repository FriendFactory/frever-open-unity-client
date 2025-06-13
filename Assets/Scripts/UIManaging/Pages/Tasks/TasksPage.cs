using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Common;
using Common.UserBalance;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.FeaturesOpening;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.VotingResult;
using UIManaging.PopupSystem;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks
{
    public sealed class TasksPage : GenericPage<TasksPageArgs>
    {
        
        public override PageId Id => PageId.TasksPage;

        [SerializeField] private TaskListView _taskListView;
        [SerializeField] private TaskListView _votingTaskListView;
        [SerializeField] private TabsManagerView _tabsManagerView;
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private SeasonLevelInfoView _userXpView;
        [SerializeField] private SeasonRewardFlowManager _seasonRewardFlowManager;
        [SerializeField] private RectTransform _topPanelRect;
        [SerializeField] private GameObject _xpBar;

        [Inject] private VideoManager _videoManager;
        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _dataHolder;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private IUnlockingLevelCreationFeatureTracker _levelCreationFeatureTracker;
        [Inject] private IVotingBattleResultManager _votingBattleResultManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        private PageManager _pageManager;
        private TaskListModelBase _taskListModel;
        private TabsManagerArgs _tabsManagerArgs;
        private bool _tabsWereInitialized;
        private CancellationTokenSource _tokenSource;
        
        private void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();
            _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
        }

        private void OnDisable()
        {
            _tokenSource.Dispose();
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------


        protected override void OnInit(PageManager pageManager)
        {
            InitializeTabs();
            _pageManager = pageManager;
        }

        protected override async void OnDisplayStart(TasksPageArgs args)
        {
            base.OnDisplayStart(args);
            _seasonRewardFlowManager.Initialize();

            _tokenSource = new CancellationTokenSource();
            
            _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
            _seasonRewardFlowManager.FlowCompleted += Refresh;

            _tabsManagerView.Init(_tabsManagerArgs);
            _tabsManagerArgs.SetSelectedTabIndex(args.TabIndex);
            OnTabSelectionCompleted(args.TabIndex);

            _xpBar.SetActive(_dataFetcher.CurrentSeason != null);
            _userXpView.Initialize(new SeasonLevelInfoStaticModel(_dataHolder));
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_topPanelRect);
            
            var task = OpenPageArgs.OpenedWithTask;
            if (task != null && (task.SoftCurrencyPayout != 0 || task.XpPayout != 0) && task.Deadline > DateTime.UtcNow)
            {
                var config = new TaskCompletedPopupConfiguration(task, StartTaskRewardAnimation);
                _popupManager.PushPopupToQueue(config);
            }
            else
            {
                await _dataHolder.UpdateBalance(_tokenSource.Token);
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _tokenSource.Cancel();
            
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
            _seasonRewardFlowManager.FlowCompleted -= Refresh;

            _taskListModel?.CleanUp();
            _taskListView.CleanUp();
            _votingTaskListView.CleanUp();
            
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void Refresh(bool transition)
        {
            _taskListModel.ReloadData();
            
            if (_levelCreationFeatureTracker.IsFeatureUnlockedInThisSession && !_levelCreationFeatureTracker.WasUserNotified)
            {
                _levelCreationFeatureTracker.NotifyUser();
            }
            
            while (_taskListModel.AwaitingData)
            {
                await Task.Delay(25, _tokenSource.Token);
            }
            
            if(_tokenSource.Token.IsCancellationRequested)
                    return;
            
            _taskListView.ReloadData();

            if (transition)
            {
                var seasonPageArgs = new SeasonPageArgs(0);
                _pageManager.MoveNext(seasonPageArgs);
            }
        }

        private void InitializeTabs()
        {
            if (_tabsWereInitialized)
            {
                return;
            }

            var tabModels = new []
            {
                new TabModel(Constants.TasksPageTabs.STYLE_BATTLES, "Style challenges"),
                new TabModel(Constants.TasksPageTabs.IN_VOTING, "In voting", true, GetInVotingTasksCount)
            };

            _tabsManagerArgs = new TabsManagerArgs(tabModels);
            _tabsWereInitialized = true;
        }

        private void OnTabSelectionCompleted(int tabIndex)
        {
            switch (tabIndex)
            {
                case Constants.TasksPageTabs.STYLE_BATTLES:
                    ShowTasksTab();
                    break;
                case Constants.TasksPageTabs.IN_VOTING:
                    ShowInVotingTaskTab();
                    break;
            }
        }

        private void ShowTasksTab()
        {
            _taskListModel?.CleanUp();
            _taskListModel = new TaskListModel(_bridge, _videoManager, _pageManager);
            
            _votingTaskListView.CleanUp();
            _votingTaskListView.SetActive(false);
            
            _taskListView.SetActive(true);
            _taskListView.Initialize(_taskListModel);
        }

        private void ShowInVotingTaskTab()
        {
            _taskListModel?.CleanUp();
            _taskListModel = new InVotingTaskListModel(_votingBattleResultManager, _bridge, _videoManager, _pageManager, _popupManager);
           
            _taskListView.CleanUp();
            _taskListView.SetActive(false);
            
            _votingTaskListView.SetActive(true);
            _votingTaskListView.Initialize(_taskListModel);
        }

        private void StartTaskRewardAnimation(object _)
        {
            var task = OpenPageArgs.OpenedWithTask;
            OpenPageArgs.OpenedWithTask = null;
            
            _seasonRewardFlowManager.Run(task);
        }

        private async Task<int> GetInVotingTasksCount()
        {
            var resp = await _bridge.GetJoinedVotingTasksCount(_tokenSource.Token);
            if (resp.IsRequestCanceled || resp.IsError) return -1;
            return resp.Count;
        }
    }
}
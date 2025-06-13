using System;
using System.Linq;
using System.Threading;
using Bridge;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks
{
    public sealed class OnboardingTasksPage : GenericPage<OnboardingTasksPageArgs>
    {
        public override PageId Id => PageId.OnboardingTasksPage;

        [SerializeField] private TaskListView _taskListView;
        [SerializeField] private SeasonRewardFlowManager seasonRewardFlowManager;
        [SerializeField] private RectTransform _topPanelRect;

        [Inject] private VideoManager _videoManager;
        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _dataHolder;
        
        private PageManager _pageManager;
        private OnboardingTaskListModel _taskListModel;
        private CancellationTokenSource _tokenSource;
        
        private void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _tokenSource.Dispose();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------


        protected override void OnInit(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        protected override void OnDisplayStart(OnboardingTasksPageArgs args)
        {
            base.OnDisplayStart(args);

            seasonRewardFlowManager.Initialize();
            _tokenSource = new CancellationTokenSource();

            if (_taskListModel != null)
            {
                _taskListModel.TaskVideoClicked -= OnVideoClicked;
                _taskListModel.CleanUp();
            }
            
            _taskListModel = new OnboardingTaskListModel(_videoManager, _pageManager, _bridge);
            _taskListModel.TaskVideoClicked += OnVideoClicked;
            
            _taskListView.Initialize(_taskListModel);
            _taskListView.gameObject.SetActive(true);

            if (OpenPageArgs.ClaimedFirstReward)
            {
                OpenPageArgs.RewardCompleted?.Invoke();
                return;
            }
            
            if (OpenPageArgs.OpenedWithTask == null)
            {
                Debug.LogError("First onboarding task not found");
                return;
            }
            
            seasonRewardFlowManager.Run(OpenPageArgs.OpenedWithTask);
            OpenPageArgs.OpenedWithTask = null;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_topPanelRect);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _tokenSource.Cancel();
            
            _taskListModel.TaskVideoClicked -= OnVideoClicked;

            _taskListModel.CleanUp();
            _taskListView.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnFlowCompleted(bool transition)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            seasonRewardFlowManager.FlowCompleted -= OnFlowCompleted;
            
            OpenPageArgs.RewardCompleted?.Invoke();
        }

        private void OnVideoClicked()
        {
            var firstTask = _taskListModel.Tasks.First();
            var firstVideo = firstTask.LevelPreviewArgs.First();
            
            OpenPageArgs.MoveNext?.Invoke(firstTask.Task.Id, firstVideo.Video.Id);
        }

    }
}
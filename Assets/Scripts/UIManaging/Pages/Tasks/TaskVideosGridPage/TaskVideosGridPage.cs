using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks.TaskVideosGridPage
{
    public sealed class TaskVideosGridPage : GenericPage<TaskVideosGridPageArgs>
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private TaskVideosGrid _videosGrid;

        [Inject] private IBridge _bridge;
        [Inject] private VideoManager _videoManager;
        [Inject] private PageManager _pageManager;

        private TaskModel _videoListListLoader;
        private TaskFullInfo _taskFullInfo;
        private CancellationTokenSource _cancellationTokenSource;

        public override PageId Id => PageId.TaskVideoGrid;

        //---------------------------------------------------------------------
        // Page
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClick);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClick);
        }

        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(TaskVideosGridPageArgs args)
        {
            base.OnDisplayStart(args);
            SetupUI();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _cancellationTokenSource.CancelAndDispose();
            _videoListListLoader.Reset();
            _videosGrid.CleanUp();
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBackButtonClick()
        {
            _pageManager.MoveBack();
        }

        private async void SetupUI()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await LoadTaskFullInfo(_cancellationTokenSource.Token);
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                SetupVideoGrid();
            }
        }

        private async Task LoadTaskFullInfo(CancellationToken token)
        {
            var result = await _bridge.GetTaskFullInfoAsync(OpenPageArgs.TaskId, token);
            if (result.IsError)
                Debug.LogError(result.ErrorMessage);

            _taskFullInfo = result.Model;
        }

        private void SetupVideoGrid()
        {
            _videoListListLoader = new TaskModel(_videoManager, _pageManager, _bridge, _taskFullInfo);
            _videosGrid.Initialize(_videoListListLoader);
        }
    }
}
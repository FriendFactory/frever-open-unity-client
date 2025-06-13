using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using EnhancedUI.EnhancedScroller;
using Extensions;
using JetBrains.Annotations;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Tasks;
using UIManaging.Pages.Tasks.TaskVideosGridPage;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class ChallengesListView : MonoBehaviour, IEnhancedScrollerDelegate
    {
        private const int ITEMS_PER_PAGE = 5;

        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _itemPrefab;
        [Header("Pagination")]
        [SerializeField] private int _loadingThresholdRows = 2;
        [Header("Results")]
        [SerializeField] private TextMeshProUGUI _noMatchText;

        private readonly List<TaskModel> _challenges = new List<TaskModel>();

        private float _cellHeight;
        private int _currentPage = 1;
        private string _nameFilter;

        private CancellationTokenSource _cancellationTokenSource;
        private ChallengesLoader _loader;
        private IBridge _bridge;
        private VideoManager _videoManager;
        private PageManager _pageManager;
        private bool _awaitingData;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private long? LastLoadedItemId => _challenges.LastOrDefault()?.Task.Id;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, PageManager pageManager, VideoManager videoManager)
        {
            _bridge = bridge;
            _pageManager = pageManager;
            _videoManager = videoManager;
            _loader = new ChallengesLoader(bridge, ITEMS_PER_PAGE);
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _cellHeight = _itemPrefab.GetComponent<RectTransform>().rect.height;
        }

        private void OnDestroy()
        {
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(string filter)
        {
            this.SetActive(true);

            CancelTemplatesLoading();
            _cancellationTokenSource = new CancellationTokenSource();

            _nameFilter = filter;
            DownloadFirstPage(); ;
        }

        public void Hide()
        {
            CancelTemplatesLoading();
            this.SetActive(false);
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_itemPrefab);
            var taskView = cellView.GetComponent<TaskViewBase>();
            var taskModel = _challenges[dataIndex];

            taskView.Initialize(taskModel);
            taskView.SetParentScrollRect(_enhancedScroller.ScrollRect);

            return cellView;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellHeight;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _challenges?.Count ?? 0;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (_awaitingData) return;

            var rowsToEnd = (int) (_enhancedScroller.NormalizedScrollPosition * _enhancedScroller.NumberOfCells);
            if (rowsToEnd > _loadingThresholdRows) return;

            DownloadNextPage();
        }

        private async void DownloadFirstPage()
        {
            _awaitingData = true;

            _currentPage = 1;
            _challenges.Clear();
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            _enhancedScroller.scrollerScrolled += ScrollerScrolled;

            var downloadedChallenges = await DownloadChallenges(LastLoadedItemId);

            if (downloadedChallenges != null)
            {
                OnPageDownloaded(downloadedChallenges);
                UpdateNoMatchText(downloadedChallenges);
            }
            else
            {
                OnPageLoadingCanceled();
            }
        }

        private async void DownloadNextPage()
        {
            _awaitingData = true;

            var downloadedTasks = await DownloadChallenges(LastLoadedItemId);
            _currentPage++;

            OnPageDownloaded(downloadedTasks);
        }

        private void OnPageDownloaded(IReadOnlyList<TaskInfo> downloadedTasks)
        {
            _awaitingData = false;

            var skipFirstOne = _currentPage > 1 && downloadedTasks.Count > 0 && downloadedTasks[0].Id == LastLoadedItemId;
            var startIndex = skipFirstOne ? 1 : 0;

            for (var i = startIndex; i < downloadedTasks.Count; i++)
            {
                var taskInfo = downloadedTasks[i];
                var taskModel = new TaskModel(_videoManager, _pageManager, _bridge, taskInfo);
                _challenges.Add(taskModel);
            }

            if (_currentPage == 1)
            {
                _enhancedScroller.ReloadData();
            }
            else
            {
                _enhancedScroller._Resize(true);
                _enhancedScroller._RefreshActive();
            }

            if (downloadedTasks.Count < ITEMS_PER_PAGE)
            {
                _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            }
        }

        private void OnPageLoadingCanceled()
        {
            _awaitingData = false;
        }

        private void UpdateNoMatchText(TaskInfo[] downloadedChallenges)
        {
            var isGridEmpty = _challenges.Count == 0 && downloadedChallenges.Length == 0;
            _noMatchText.SetActive(isGridEmpty);
        }

        private async Task<TaskInfo[]> DownloadChallenges(object targetId)
        {
            var token = _cancellationTokenSource.Token;
            return await _loader.DownloadChallenges(targetId, _nameFilter, token);
        }

        private void CancelTemplatesLoading()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }
    }
}
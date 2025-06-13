using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Template;
using Common;
using EnhancedUI.EnhancedScroller;
using Extensions;
using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Common.Templates
{
    public sealed class TemplatesListView : TemplatesGridScrollerDelegate
    {
        [Header("Pagination")]
        [SerializeField] private int _loadingThresholdRows = 2;
        [Header("Results")]
        [SerializeField] private TextMeshProUGUI _noMatchText;

        private int _currentPage = 1;
        private string _nameFilter;

        private CancellationTokenSource _cancellationTokenSource;
        [Inject] private TemplatesLoader _loader;
        private bool _awaitingData;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private int ItemsPerPage => Constants.Templates.TEMPLATE_PAGE_SIZE;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            CancelTemplatesLoading();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(string filter = "")
        {
            this.SetActive(true);

            CancelTemplatesLoading();
            _cancellationTokenSource = new CancellationTokenSource();

            _nameFilter = filter;
            DownloadFirstPage();
        }

        public void Hide()
        {
            CancelTemplatesLoading();
            this.SetActive(false);
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
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            _enhancedScroller.scrollerScrolled += ScrollerScrolled;
            Templates.Clear();

            var downloadedTemplates = await DownloadTemplates(0);
            if (downloadedTemplates != null)
            {
                OnPageDownloaded(downloadedTemplates);

                var isGridEmpty = Templates.Count == 0 && downloadedTemplates.Length == 0;
                _noMatchText.SetActive(isGridEmpty);
            }
            else
            {
                _noMatchText.SetActive(false);
                OnPageLoadingCanceled();
            }
        }

        private async void DownloadNextPage()
        {
            _awaitingData = true;

            var skip = _currentPage * ItemsPerPage;
            var downloadedTemplates = await DownloadTemplates(skip);

            _currentPage++;

            OnPageDownloaded(downloadedTemplates);
        }

        private void OnPageDownloaded(IReadOnlyCollection<TemplateInfo> downloadedTemplates)
        {
            _awaitingData = false;

            Templates.AddRange(downloadedTemplates);

            if (_currentPage == 1)
            {
                _enhancedScroller.Delegate = this;
                _enhancedScroller.ReloadData();
            }
            else
            {
                _enhancedScroller._Resize(true);
                _enhancedScroller._RefreshActive();
            }

            if (downloadedTemplates.Count == 0)
            {
                _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            }
        }

        private void OnPageLoadingCanceled()
        {
            _awaitingData = false;
        }

        private void CancelTemplatesLoading()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        private Task<TemplateInfo[]> DownloadTemplates(int skip)
        {
            var token = _cancellationTokenSource.Token;
            if (string.IsNullOrEmpty(_nameFilter))
            {
                return _loader.GetPersonalisedTemplates(skip, ItemsPerPage, cancellationToken: token);
            }
            
            return _loader.DownloadTemplates(skip, ItemsPerPage,null, _nameFilter, token);
        }
    }
}
using System;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Template;
using EnhancedUI.EnhancedScroller;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Templates;
using UnityEngine;
using Zenject;
using static UIManaging.Common.Templates.TemplatesLoader;

namespace UIManaging.Pages.CreatePostPage.TemplateGallery
{
    internal class TemplatesGrid : TemplatesGridScrollerDelegate
    {
        public event Action OnScrolled;
        public event Action OnFirstPageLoaded;
        
        private const int ITEMS_PER_PAGE = 18;
        
        [Header("Pagination")]
        [SerializeField] private int _loadingThresholdRows = 2;
        [Header("Results")]
        [SerializeField] private GameObject _noResultsMessage;
        [SerializeField] private AnimatedSkeletonBehaviour _skeletonLoading;

        private TemplatesLoader _loader;
        private RectTransform _rectTransform;

        private int _currentPage = 1;
        private bool _awaitingData;
        private bool _skeletonDisplayed;
        
        private string _nameFilter = string.Empty;
        private TemplateCategory _templateCategory;
        private TemplateSubCategory _templateSubCategory;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public int? CharactersCount { get; set;  }

        public bool FirstPageLoaded => Templates.Count > 0;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, AmplitudeManager amplitudeManager)
        {
            _loader = new TemplatesLoader(bridge, amplitudeManager);
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void DownloadTemplates(int? charactersCount, string filter)
        {
            CharactersCount = charactersCount;
            _nameFilter = filter;

            DownloadFirstPage();
        }
        
        public void DownloadTemplates(int? charactersCount)
        {
            DownloadTemplates(charactersCount, _nameFilter);
        }

        public void DownloadTemplates(string filter)
        {
            DownloadTemplates(CharactersCount, filter);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            OnScrolled?.Invoke();
            
            if (_awaitingData) return;
            var rowsToEnd = (int) (_enhancedScroller.NormalizedScrollPosition * _enhancedScroller.NumberOfCells);
            if (rowsToEnd > _loadingThresholdRows) return;

            DownloadNextPage();
        }

        private async void DownloadFirstPage()
        {
            _noResultsMessage.SetActive(false);

            if (!_skeletonDisplayed)
            {
                _skeletonDisplayed = true;
                _skeletonLoading.Play();
            }
            
            _awaitingData = true;

            _currentPage = 1;
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            _enhancedScroller.scrollerScrolled += ScrollerScrolled;
            Templates.Clear();

            var downloadedTemplates = await _loader.DownloadTemplates(0, ITEMS_PER_PAGE, CharactersCount, _nameFilter);
            
            if (this.IsDestroyed()) return;
            
            _skeletonLoading.FadeOut();
            OnPageDownloaded(downloadedTemplates);
            var isGridEmpty = Templates.Count == 0 && downloadedTemplates.Length == 0;
            _noResultsMessage.SetActive(isGridEmpty);
            
            OnFirstPageLoaded?.Invoke();
        }

        private async void DownloadNextPage()
        {
            _awaitingData = true;

            var skip = _currentPage * ITEMS_PER_PAGE;
            var downloadedTemplates = await _loader.DownloadTemplates(skip, ITEMS_PER_PAGE, CharactersCount, _nameFilter);

            _currentPage++;

            OnPageDownloaded(downloadedTemplates);
        }

        private void OnPageDownloaded(TemplateInfo[] downloadedTemplates)
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

            if (downloadedTemplates.Length == 0)
            {
                _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            }
        }
    }
}
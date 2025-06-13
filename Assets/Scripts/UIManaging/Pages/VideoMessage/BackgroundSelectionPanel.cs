using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using EnhancedUI.EnhancedScroller;
using Extensions;
using ModestTree;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VideoMessage
{
    internal sealed class BackgroundSelectionPanel : MonoBehaviour, IEnhancedScrollerDelegate
    {
        private const int STATIC_CELLS_COUNT = 1;
        private const int THRESHOLD_TO_LOAD_NEXT_PAGE = 8;

        [Header("Static Cells")]
        [SerializeField] private EnhancedScrollerCellView _uploadCustomBackgroundButton;
        [Header("Dynamic Cells")]
        [SerializeField] private BackgroundView _backgroundViewPrefab;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [Header("Image Generation")]
        [SerializeField] private BackgroundGenerationPanel _backgroundGenerationPanel;

        private readonly List<BackgroundView> _views = new List<BackgroundView>();
        private readonly List<IBackgroundOption> _backgrounds = new List<IBackgroundOption>();

        [Inject] private ILevelManager _levelManager;
        [Inject] private SetLocationBackgroundListProvider _backgroundListProvider;
        [Inject] private SetLocationBackgroundThumbnailProvider _backgroundThumbnailProvider;

        private int _pageIndex;
        private int? _loadingPageIndex;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Init()
        {
            _uploadCustomBackgroundButton.SetActive(false);

            var backgrounds = _backgroundListProvider.HasCached
                ? _backgroundListProvider.CachedBackgrounds.ToArray()
                : await _backgroundListProvider.GetSetLocationBackgrounds(_pageIndex);

            _enhancedScroller.Delegate = this;
            RegisterSetLocationBackgrounds(backgrounds);
            _enhancedScroller.scrollerScrolled += OnScroll;
            _levelManager.PhotoOnSetLocationChanged += UnselectAllBackgroundViews;
            var currentBackground = _levelManager.TargetEvent.GetSetLocationBackground();
            var backgroundModel = backgrounds.FirstOrDefault(x => x.Id == currentBackground?.Id);
            var scrollViewIndex = backgroundModel == null ? 0 : backgrounds.IndexOf(backgroundModel) + STATIC_CELLS_COUNT; // for upload button + AI buttons
            _pageIndex = scrollViewIndex / _backgroundListProvider.PageSize;
            _enhancedScroller.ReloadData();
            _enhancedScroller.JumpToDataIndex(scrollViewIndex);
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _backgrounds.Count + STATIC_CELLS_COUNT; // for upload button + AI buttons
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            Component view;

            switch (dataIndex)
            {
                case 0:
                    view = _uploadCustomBackgroundButton;
                    break;
                default:
                    view = _backgroundViewPrefab;
                    break;
            }

            return view.GetComponent<RectTransform>().GetWidth();
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            switch (cellIndex)
            {
                case 0:
                    _uploadCustomBackgroundButton.SetActive(true);
                    return _uploadCustomBackgroundButton;
            }

            var view = scroller.GetCellView(_backgroundViewPrefab).GetComponent<BackgroundView>();
            view.Clicked -= OnClicked;
            view.Clicked += OnClicked;
            var model = _backgrounds[dataIndex - STATIC_CELLS_COUNT];
            view.Init(model, _backgroundThumbnailProvider);
            view.SetSelected(_levelManager.TargetEvent.GetFreverBackgroundId() == model.Id);
            _views.Add(view);
            return view;
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            foreach (var view in _views)
            {
                Destroy(view.Texture);
            }

            _levelManager.PhotoOnSetLocationChanged -= UnselectAllBackgroundViews;
            _backgroundThumbnailProvider.Cleanup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RegisterSetLocationBackgrounds(IBackgroundOption[] backgrounds)
        {
            _backgrounds.AddRange(backgrounds);
            _enhancedScroller._Resize(true);
        }

        private void OnClicked(IBackgroundOption background)
        {
            switch (background.Type)
            {
                case BackgroundOptionType.Image:
                    _levelManager.ApplySetLocationBackground((SetLocationBackground)background);
                    break;
                case BackgroundOptionType.GenerationSettings:
                    var backgroundSettings = (SetLocationBackgroundSettings)background;
                    _backgroundGenerationPanel.OpenGenerationPopup(backgroundSettings);
                    break;
                default:
                    Debug.LogWarning("Unsupported background type: " + background.Type);
                    break;
            }

            foreach (var view in _views)
            {
                view.SetSelected(view.BackgroundId == background.Id);
            }
        }

        private async void OnScroll(EnhancedScroller scroller, Vector2 val, float scrollposition)
        {
            var current = scroller.GetCellViewIndexAtPosition(scrollposition);
            if (current < _backgrounds.Count - THRESHOLD_TO_LOAD_NEXT_PAGE) return;

            var nextPage = _pageIndex + 1;
            var alreadyLoading = _loadingPageIndex == nextPage;
            if (alreadyLoading) return;
            _loadingPageIndex = nextPage;
            var backgrounds = await _backgroundListProvider.GetSetLocationBackgrounds(nextPage);
            _loadingPageIndex = null;
            if (backgrounds.IsNullOrEmpty()) return;
            _pageIndex = nextPage;
            RegisterSetLocationBackgrounds(backgrounds);
        }

        private void UnselectAllBackgroundViews()
        {
            foreach (var view in _views)
            {
                view.SetSelected(false);
            }
        }
    }
}
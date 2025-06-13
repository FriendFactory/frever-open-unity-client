using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Bridge.Models.VideoServer;
using DG.Tweening;
using Extensions;
using Navigation.Args;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Hashtags;
using UIManaging.Common.SearchPanel;
using UIManaging.Common.Templates;
using UIManaging.Localization;
using UIManaging.Pages.Common.TabsManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Navigation.Args.DiscoverySearchState;

namespace UIManaging.Pages.DiscoveryPage
{
    public class DiscoverySearchView : MonoBehaviour
    {
        public event Action<HashtagInfo> HashtagClicked;

        [SerializeField] private CanvasGroup _canvasGroup;
        [Space]
        [SerializeField] private SearchPanelView _searchPanel;
        [SerializeField] private TabsManagerView _tabsManagerView;
        [Space]
        [SerializeField] private ChallengesListView _challengesPanel;
        [SerializeField] private SearchHandler _creatorsPanel;
        [SerializeField] private TemplatesListView _templatesPanel;
        [SerializeField] private HashtagsPanel _hashtagsPanel;
        [SerializeField] private CrewsPanel _crewsPanel;
        [Space]
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _backButton;
        [Space]
        [SerializeField] private AnimatedTabUnderlineBehaviour _animatedTabUnderline;

        [Inject] private DiscoveryPageLocalization _localization;
        
        private DiscoveryPageArgs _pageArgs;
        private bool _tabsWereInitialized;
        private TabsManagerArgs _tabsManagerArgs;
        private Tween _searchBarAnimationSequence;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Init(DiscoveryPageArgs args)
        {
            _pageArgs = args;

            InitializeTabs();
            _cancelButton.onClick.AddListener(OnSearchCancel);
            
            // needed so Unity can update tabs positions otherwise tab underline animation failed
            gameObject.SetActive(true);
            await Task.Delay(100);
            gameObject.SetActive(false);
            _canvasGroup.alpha = 1.0f;

            var searchRect = _searchPanel.GetComponent<RectTransform>();
            
            var delta = searchRect.sizeDelta;
            var pos = searchRect.anchoredPosition;
            
            var targetDelta = delta - new Vector2(80f, 0f);
            var targetPos = pos - new Vector2(120f, 0f);
            
            _searchBarAnimationSequence = DOTween.Sequence()
                                                 .Append(searchRect.DOSizeDelta(targetDelta, 0.15f))
                                                 .Join(searchRect.DOAnchorPos(targetPos, 0.15f))
                                                 .SetAutoKill(false)
                                                 .Pause();

            _searchPanel.OnSelect.AddListener(OnSearchSelect);

            if (_pageArgs.SearchState != Disabled)
            {
                Show(_pageArgs.SearchState, _pageArgs.SearchText);
            }
            else
            {
                _searchPanel.PlaceholderText = _localization.DefaultSearchInputPlaceholder;
            }
        }

        public void Show(DiscoverySearchState searchState, string searchText = "")
        {
            _tabsManagerView.TabsManagerArgs.SetSelectedTabIndex((int) searchState);
            _animatedTabUnderline.SetTargetTabIndexImmediate((int) searchState);
            this.SetActive(true);
            _cancelButton.SetActive(true);
            _backButton.SetActive(false);
            _searchPanel.SetTextWithoutNotify(searchText);
            OnTabSelectionCompleted((int) searchState, true);
            _searchBarAnimationSequence.PlayForward();
        }

        public void Hide()
        {
            _pageArgs.SearchText = _searchPanel.Text;

            _searchPanel.ClearWithoutNotify();
            this.SetActive(false);
            _cancelButton.SetActive(false);
            _backButton.SetActive(true);
            _searchBarAnimationSequence.PlayBackwards();
            _searchPanel.PlaceholderText = _localization.DefaultSearchInputPlaceholder;
            _animatedTabUnderline.SetTargetTabIndexImmediate(0);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InitializeTabs()
        {
            if (_tabsWereInitialized) return;

            var tabModels = new []
            {
                new TabModel((int) Users, _localization.UsersSearchTab),
                new TabModel((int) Templates, _localization.SoundsSearchTab),
                new TabModel((int) Hashtags, _localization.HashtagsSearchTab),
                new TabModel((int) Crews, _localization.CrewsSearchTab),
            };

            _tabsManagerArgs = new TabsManagerArgs(tabModels);
            _tabsWereInitialized = true;
            _tabsManagerView.Init(_tabsManagerArgs);
            _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
            _animatedTabUnderline.Initialize();
        }
        
        private void OnSearchSelect(string input)
        {
            if (!gameObject.activeInHierarchy) Show(Users);
        }

        private void OnSearchCancel()
        {
            _pageArgs.SearchState = Disabled;
            _pageArgs.SearchText = string.Empty;
            Hide();
        }

        private void OnTabSelectionCompleted(int tabIndex)
        {
            _animatedTabUnderline.SetTargetTabIndex(tabIndex);
            _animatedTabUnderline.Play(null);
            OnTabSelectionCompleted(tabIndex, false);
        }

        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
        private void OnTabSelectionCompleted(int tabIndex, bool forceUpdate)
        {
            var searchState = (DiscoverySearchState) tabIndex;
            if (!forceUpdate && _pageArgs.SearchState == searchState) return;

            HideAll();
            UnsubscribeFromSearchEvents();
            
            switch (searchState)
            {
                case Disabled:
                    _searchPanel.PlaceholderText = _localization.DefaultSearchInputPlaceholder;
                    break;
                case Users:
                    _creatorsPanel.SetActive(true);
                    _creatorsPanel.SetSearchHandling(true);
                    _searchPanel.PlaceholderText = _localization.UsersSearchInputPlaceholder;
                    break;
                case Templates:
                    _templatesPanel.Show(_searchPanel.Text);
                    _searchPanel.PlaceholderText = _localization.SoundsSearchInputPlaceholder;
                    UpdateSearchEvents(HandleTemplateSearch, HandleTemplateSearchCleared);
                    break;
                case Hashtags:
                    _hashtagsPanel.Show(_searchPanel.Text);
                    _hashtagsPanel.HashtagItemClicked += OpenHashtagFeed;
                    _searchPanel.PlaceholderText = _localization.HashtagsSearchInputPlaceholder;
                    UpdateSearchEvents(HandleHashtagSearch, HandleHashtagSearchCleared);
                    break;
                case Crews:
                    _crewsPanel.Show(_searchPanel.Text, false);
                    _searchPanel.PlaceholderText = _localization.CrewsSearchInputPlaceholder;
                    break;
            }

            _pageArgs.SearchState = searchState;
        }

        private void HideAll()
        {
            _challengesPanel.Hide();

            _creatorsPanel.SetActive(false);
            _creatorsPanel.SetSearchHandling(false);

            _templatesPanel.Hide();

            _hashtagsPanel.Hide();
            _hashtagsPanel.HashtagItemClicked -= OpenHashtagFeed;

            _crewsPanel.Hide();
        }

        //---------------------------------------------------------------------
        // Search
        //---------------------------------------------------------------------

        private void UnsubscribeFromSearchEvents()
        {
            _searchPanel.InputCompleted -= HandleTemplateSearch;
            _searchPanel.InputCompleted -= HandleHashtagSearch;
            _searchPanel.InputCleared -= HandleTemplateSearchCleared;
            _searchPanel.InputCleared -= HandleHashtagSearchCleared;
        }

        private void UpdateSearchEvents(Action<string> onInputCompleted, Action onInputCleared)
        {
            _searchPanel.InputCompleted += onInputCompleted;
            _searchPanel.InputCleared += onInputCleared;
        }
        
        //---------------------------------------------------------------------
        // Templates
        //---------------------------------------------------------------------

        private void HandleTemplateSearch(string input)
        {
            _templatesPanel.Show(input);
        }

        private void HandleTemplateSearchCleared()
        {
            _templatesPanel.Show(string.Empty);
        }

        //---------------------------------------------------------------------
        // Hashtags
        //---------------------------------------------------------------------

        private void HandleHashtagSearch(string input)
        {
            _hashtagsPanel.Show(input);
        }

        private void HandleHashtagSearchCleared()
        {
            _hashtagsPanel.Show(string.Empty);
        }

        private void OpenHashtagFeed(HashtagInfo obj)
        {
            HashtagClicked?.Invoke(obj);
        }
    }
}
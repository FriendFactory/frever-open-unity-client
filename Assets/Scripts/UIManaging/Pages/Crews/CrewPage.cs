using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Extensions;
using Modules.Crew;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.SearchPanel;
using UIManaging.Localization;
using UIManaging.Pages.Common.Panels;
using UnityEngine;
using UnityEngine.UI;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.DiscoveryPage;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewPage : GenericPage<CrewPageArgs>
    {
        private const int CHAT_TAB_ID = 0;
        private const int TROPHY_TAB_ID = 1;
        private const int ABOUT_TAB_ID = 2;

        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _crewName;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _createButton;
        [SerializeField] private PostTypeSelectionPanel _postTypeSelectionPanel;
        [SerializeField] private Button _closePostTypePanelButton;
        [SerializeField] private Button _searchButton;
        [SerializeField] private SearchPanelView _searchPanelView;
        [SerializeField] private CrewsPanel _crewsPanel;
        
        [Header("Tabs")] 
        [SerializeField] private TabsManagerView _tabsManagerView;
        [SerializeField] private CrewTabContent[] _tabs;

        [Space] 
        [SerializeField] private SlideInOutBehaviour _slideInOutBehaviour;
        [SerializeField] private SlideInOutBehaviour _crewSearchAnimator;

        [Inject] private CrewService _crewService;
        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private CrewPageLocalization _localization;

        private TabsManagerArgs _tabsManagerArgs;
        private bool _tabsWereInitialized;
        private bool _searchEnabled;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.CrewPage;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _createButton.onClick.AddListener(_postTypeSelectionPanel.Show);
            _closePostTypePanelButton.onClick.AddListener(ClosePostTypeSelectionPanel);
            _searchButton.onClick.AddListener(ToggleSearch);
        }

        private void OnDisable()
        {
            _crewService.CrewModelUpdated -= OnCrewModelUpdated;
            _crewService.SidebarDisabled -= OnSidebarSlideOutStart;
            _closePostTypePanelButton.onClick.RemoveListener(ClosePostTypeSelectionPanel);
            
            _optionsButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
            _createButton.onClick.RemoveAllListeners();
            _searchButton.onClick.RemoveAllListeners(); 
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _searchPanelView.InputCleared += () => OpenPageArgs.SearchQuery = string.Empty;
            _searchPanelView.InputCompleted += searchQuery => OpenPageArgs.SearchQuery = searchQuery;
        }

        protected override async void OnDisplayStart(CrewPageArgs args)
        {
            base.OnDisplayStart(args);

            _crewService.CrewModelUpdated += OnCrewModelUpdated;
            _crewService.SidebarDisabled += OnSidebarSlideOutStart;
            
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _optionsButton.onClick.AddListener(OnOptionsButtonClick);
            
            _slideInOutBehaviour.InitSequence(Vector3.zero, new Vector3(-978,0));
            
            await _localUser.DownloadProfile();
            await _crewService.RefreshCrewDataAsync(default);
            
            if (IsDestroyed) return;
            
            InitializeTabs();
            
            if (OpenPageArgs.OpenJoinRequests)
            {
                _crewService.OpenJoinRequests();
            }
            
            _crewName.text = _crewService.Model?.Name;
            InitializePostTypeSelectionPanel();
            
            if (!string.IsNullOrEmpty(args.SearchQuery))
            {
                ShowSearchPanel();
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InitializeTabs()
        {
            if (_tabsWereInitialized) return;

            var tabModels = new[]
            {
                new TabModel(CHAT_TAB_ID, _localization.TabChat),
                new TabModel(TROPHY_TAB_ID, _localization.TabTrophyHunt),
                new TabModel(ABOUT_TAB_ID, _localization.TabAbout)
            };

            _tabsManagerArgs = new TabsManagerArgs(tabModels);
            _tabsWereInitialized = true;
            _tabsManagerView.Init(_tabsManagerArgs);
            _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;

            OnTabSelectionCompleted(CHAT_TAB_ID);
        }

        private void OnTabSelectionCompleted(int tabIndex)
        {
            var selectedTab = _tabs[tabIndex];

            var initialize = !selectedTab.IsInitialized && _crewService.Model != null;
            if (initialize)
            {
                selectedTab.Initialize(_crewService.Model);
            }

            for (var i = 0; i < _tabs.Length; i++)
            {
                if (i == tabIndex)
                {
                    _tabs[i].Show();
                }
                else
                {
                    _tabs[i].Hide();
                }
            }
        }

        private void OnBackButtonClicked()
        {
            if (_searchEnabled)
            {
                HideSearchPanel();
                return;
            }
            Manager.MoveNext(new HomePageArgs());
        }

        private async Task<CrewModel> RequestCrewModel(long crewId, CancellationToken token)
        {
            var result = await _bridge.GetCrew(crewId, token);

            if (result.IsSuccess) return result.Model;

            if (result.IsError) Debug.LogError(result.ErrorMessage);

            return null;
        }

        private void OnOptionsButtonClick()
        {
            _slideInOutBehaviour.SlideOut();
            _crewService.OpenCrewSidebar();
        }

        private void OnSidebarSlideOutStart()
        {
            _slideInOutBehaviour.SlideIn();
        }

        private void OnCrewModelUpdated(CrewModel crewModel)
        {
            _crewName.text = crewModel.Name;

            if (_tabsManagerArgs?.SelectedTabIndex == ABOUT_TAB_ID)
            {
                _tabs[ABOUT_TAB_ID].Initialize(crewModel);
            }
        }
        
        private async void InitializePostTypeSelectionPanel()
        {
            var res = await _bridge.GetChatById(_crewService.Model.ChatId);
            if (res.IsSuccess)
            {
                _postTypeSelectionPanel.Init(res.Model);
            }
        }
        
        private void ClosePostTypeSelectionPanel()
        {
            _postTypeSelectionPanel.Hide();
        }

        private void ToggleSearch()
        {
            if (!_searchEnabled)
            {
                ShowSearchPanel();
            }
            else
            {
                HideSearchPanel();
            }
        }
        
        private void ShowSearchPanel()
        {
            _searchEnabled = true; 
            _searchPanelView.Text = OpenPageArgs.SearchQuery;
            _crewsPanel.Show(OpenPageArgs.SearchQuery, OpenPageArgs.Backed);
            _crewSearchAnimator.SlideIn();
            _searchPanelView.Select();
            _optionsButton.SetActive(false);
            _crewName.text = _localization.SearchHeader;
            _crewService.EnableChatInput(false);
            _crewService.SidebarDisabled -= OnSidebarSlideOutStart;
        }

        private void HideSearchPanel()
        {
            _searchEnabled = false;
            _crewSearchAnimator.SlideOut();
            _searchPanelView.Clear();
            _searchPanelView.Deselect();
            _optionsButton.SetActive(true);
            _crewName.text = _crewService.Model?.Name;
            _crewService.EnableChatInput(true);
            _crewService.SidebarDisabled += OnSidebarSlideOutStart;
        }
    }
}
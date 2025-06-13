using System;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using Modules.Amplitude;
using Modules.FollowRecommendations;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Common.SearchPanel;
using UIManaging.Localization;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.FollowersPage.Recommendations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.FollowersPage.UI
{
    internal sealed class FollowersPage : GenericPage<BaseFollowersPageArgs>
    {
        public override PageId Id => PageId.FollowersPage;
        
        [SerializeField] private TabsManagerView _tabManagerView;
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private SearchHandler _searchHandler;
        [SerializeField] private FollowersSearchListView _followersSearchListView;
        [SerializeField] private Button _discoverPeopleButton;
    
        [Inject] private PageManager _pageManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private FollowersManager _followersManager;
        [Inject] private FollowRecommendationsListModelProvider _recommendationsProvider;
        [Inject] private ProfileLocalization _localization;
        
        private BaseFollowersPageArgs _baseFollowersPageArgs;
        private RemoteUserFollowersPageArgs _remoteUserFollowersPageArgs;
        private TabsManagerArgs _tabsManagerArgs;
        private CancellationTokenSource _tokenSource;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            var localUserTabsModels = new[]
            {
                new TabModel(BaseFollowersPageArgs.FRIENDS_TAB_INDEX, _localization.FriendsTab),
                new TabModel(BaseFollowersPageArgs.FOLLOWING_TAB_INDEX, _localization.FollowingTab),
                new TabModel(BaseFollowersPageArgs.FOLLOWERS_TAB_INDEX, _localization.FollowersTab)
            };
                
            _tabsManagerArgs = new TabsManagerArgs(localUserTabsModels);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _pageHeaderView.Init(new PageHeaderArgs(string.Empty, new ButtonArgs(string.Empty, _pageManager.MoveBack)));
        }
        
        protected override async void OnDisplayStart(BaseFollowersPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _tokenSource = new CancellationTokenSource();

            _baseFollowersPageArgs = args;
            _tabsManagerArgs.SetSelectedTabIndexSilent(_baseFollowersPageArgs.InitialTabIndex);
            _tabManagerView.TabSelectionCompleted += OnTabSelectionCompleted;

            _discoverPeopleButton.SetActive(_baseFollowersPageArgs.IsLocalUser);
            
            if (_baseFollowersPageArgs.IsLocalUser)
            { 
                await InitializeFollowersListViewAsync(args, _tokenSource.Token);
                await _followersManager.PrefetchDataForLocalUser();
                _searchHandler.SetTargetProfileToLocalUser();
            }
            else
            {
                _remoteUserFollowersPageArgs = (RemoteUserFollowersPageArgs)_baseFollowersPageArgs;
                await _followersManager.PrefetchDataForRemoteUser(_remoteUserFollowersPageArgs.GroupId);
                _searchHandler.SetTargetProfile(_remoteUserFollowersPageArgs.GroupId);
            }
            
            if (_tokenSource.IsCancellationRequested) return;
            
            OnTabSelectionCompleted(_tabsManagerArgs.SelectedTabIndex);
            _tabManagerView.Init(_tabsManagerArgs);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            
            _followersSearchListView.Clear();
            
            _tabManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
            _tabManagerView.Hide();

            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnTabSelectionCompleted(int tabIndex)
        {
            OpenPageArgs.InitialTabIndex = tabIndex;

            switch (tabIndex)
            {
                case BaseFollowersPageArgs.FRIENDS_TAB_INDEX:
                    ShowOnlyFriends();
                    break;
                case BaseFollowersPageArgs.FOLLOWERS_TAB_INDEX:
                    ShowOnlyFollowers();
                    break;
                case BaseFollowersPageArgs.FOLLOWING_TAB_INDEX:
                    ShowOnlyFollowing();
                    break;
            }
        }
        
        private void ShowOnlyFriends()
        {
            _searchHandler.SetUsersFilter(UsersFilter.Friends);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.FRIENDS_PAGE);
        }
        
        private void ShowOnlyFollowers()
        {
            _searchHandler.SetUsersFilter(UsersFilter.Followers);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.FOLLOWER_PAGE);
        }

        private void ShowOnlyFollowing()
        {
            _searchHandler.SetUsersFilter(UsersFilter.Followed);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.FOLLOWING_PAGE);
        }

        private async Task InitializeFollowersListViewAsync(BaseFollowersPageArgs pageArgs, CancellationToken token)
        {
            // we need to reload recommendations only when user directly navigated to Followers page
            if (!OpenPageArgs.Backed)
            {
                await _recommendationsProvider.PrefetchData(token);
            }

            var followRecommendations = _recommendationsProvider.GetListModel(FollowRecommendationsType.Friends);
            var followBackRecommendation = _recommendationsProvider.GetListModel(FollowRecommendationsType.FollowBack);
            var followersListModel = new FollowersSearchListModel(followRecommendations, followBackRecommendation,
                                                                  true, pageArgs.InitialTabIndex);
            
            _followersSearchListView.InitializeFollowers(followersListModel);
        }
    }
}
using EnhancedUI.EnhancedScroller;
using Extensions;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.FollowersPage.Recommendations;
using UnityEngine;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowersSearchListView : SearchListView
    {
        [Space] 
        [SerializeField] private TabsManagerView _tabsManagerView;
        [SerializeField] private SearchPanelView _searchPanel;
        [Space] 
        [SerializeField] private EnhancedScrollerCellView _followRecommendationsView;
        [SerializeField] private EnhancedScrollerCellView _followBackRecommendationsView;
        [SerializeField] private EnhancedScrollerCellView _emptyRecommendationsView;
        [Space] 
        [SerializeField] private FollowersEmptyListMessageHandler _followersMessageHandler;

        private float _followRecommendationsCellSize;
        private bool _keyboardVisible;
        private int _currentTabIndex;
        
        private FollowersSearchListModel FollowersModel { get; set; }
        private bool IsLocalUser => FollowersModel != null && FollowersModel.IsLocalUser;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _followRecommendationsCellSize = _followRecommendationsView.GetComponent<RectTransform>().rect.height;
        }

        private void OnEnable()
        {
            _tabsManagerView.TabSelectionCompleted += OnTabSelectionCompleted;
            _searchPanel.KeyboardVisibilityChanged += OnKeyboardVisibilityChanged;
        }

        private void OnDisable()
        {
            _tabsManagerView.TabSelectionCompleted -= OnTabSelectionCompleted;
            _searchPanel.KeyboardVisibilityChanged -= OnKeyboardVisibilityChanged;
        }

        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------

        public void InitializeFollowers(FollowersSearchListModel followersSearchListModel)
        {
            FollowersModel = followersSearchListModel;
            _currentTabIndex = FollowersModel.InitialTabIndex;
        }

        public void Clear()
        {
            FollowersModel = null;
            
            _enhancedScroller.ClearAll();
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected override void UpdateNoUserMatchText(bool isReload = false)
        {
            _followersMessageHandler.HideMessage();
            _noUserMatchText.SetActive(false);

            if (!isReload && !_searchPanel.HasInput && ContextData.Users.Length == 0)
            {
                _followersMessageHandler.ShowLocalUserMessage(IsLocalUser, (UsersFilter)_currentTabIndex + 1);
            }
            else
            {
                base.UpdateNoUserMatchText(isReload);
            }
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate implementation
        //---------------------------------------------------------------------

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            const int nonUserProfileCellsCount = 1;
            if (ContextData == null) return nonUserProfileCellsCount;
            return ContextData.Users.Length + nonUserProfileCellsCount;
        }

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (dataIndex > 0) return base.GetCellViewSize(scroller, dataIndex);

            var hideFollowRecommendation = _keyboardVisible || !IsLocalUser || _currentTabIndex == BaseFollowersPageArgs.FOLLOWERS_TAB_INDEX;
            if (hideFollowRecommendation) return 0;

            return _followRecommendationsCellSize;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex != 0) return base.GetCellView(scroller, dataIndex - 1, cellIndex);
            
            if (!IsLocalUser || _currentTabIndex == BaseFollowersPageArgs.FOLLOWERS_TAB_INDEX)
            {
                return scroller.GetCellView(_emptyRecommendationsView);
            }

            return GetFollowRecommendationsView();

        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private EnhancedScrollerCellView GetFollowRecommendationsView()
        {
            var followListModel = FollowersModel.FollowRecommendationsModel;
            var followBackListModel = FollowersModel.FollowBackRecommendationsModel;

            var showFollowBackRecommendations = _currentTabIndex == BaseFollowersPageArgs.FRIENDS_TAB_INDEX;
            
            var followRecommendationsCellView = showFollowBackRecommendations 
                ? _enhancedScroller.GetCellView(_followBackRecommendationsView)
                : _enhancedScroller.GetCellView(_followRecommendationsView);
            var followRecommendationsView = followRecommendationsCellView.GetComponent<FollowRecommendationsView>();

            if (followRecommendationsView.IsInitialized) return followRecommendationsCellView;

            followRecommendationsView.Initialize(showFollowBackRecommendations ? followBackListModel : followListModel);

            return followRecommendationsCellView;
        }
        
        private void OnTabSelectionCompleted(int tabIndex) => _currentTabIndex = tabIndex;

        private void OnKeyboardVisibilityChanged(bool visible)
        {
            _keyboardVisible = visible;
            
            _enhancedScroller.ReloadData();
        }
    }
}
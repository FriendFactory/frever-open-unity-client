using System.Collections.Generic;
using System.Linq;
using Abstract;
using Bridge.Services.UserProfile;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.FollowersPage.UI.FollowersLists
{
    public class PaginatedFollowersListView : BaseContextDataView<PaginatedFollowersListModel>, IEnhancedScrollerDelegate
    {
        private const float LOADING_NEXT_PAGE_POSITION_THRESHOLD = 0.1f;
        
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _followerViewPrefab;
        [SerializeField] private TextMeshProUGUI _noFollowersText;

        private readonly List<FollowerViewModel> _viewModels = new List<FollowerViewModel>();
        private bool _isDownloading;
        private float _cellSize;

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _cellSize = _followerViewPrefab.GetComponent<RectTransform>().rect.height;
            
        }
        
        protected override void OnInitialized()
        {
            _noFollowersText.gameObject.SetActive(false);
            _viewModels.Clear();
            
            if (ContextData.Profiles.Count > 0)
            {
                var existingProfiles = ContextData.Profiles.Select(profile => new FollowerViewModel(profile)).ToArray();
                _viewModels.AddRange(existingProfiles);
            }
            else
            {
                DownloadNextPage();
            }
            
            _enhancedScroller.ReloadData();
            _enhancedScroller.scrollerScrolled += ScrollerScrolled;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _viewModels.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_followerViewPrefab);
            var followerView = cellView.GetComponent<FollowerView>();
            followerView.Initialize(_viewModels[dataIndex]);
            return cellView;
        }

        public void UpdateFollower(Profile profile)
        {
            var modelToUpdate = _viewModels.FirstOrDefault(x => x.GroupId == profile.MainGroupId);
            modelToUpdate?.SetProfile(profile);
        }

        private async void DownloadNextPage()
        {
            _isDownloading = true;
            var followerViews = await ContextData.GetNextFollowerViewModels(skip: _viewModels.Count);
            
            if(followerViews.Length == 0) return;
            
            _viewModels.AddRange(followerViews);
            
            _enhancedScroller._Resize(true);
            _enhancedScroller._RefreshActive();
            _isDownloading = false;

            var showNoFollowersText = ContextData.IsLocalUser && _viewModels.Count == 0;
            ShowNoFollowersText(showNoFollowersText);
        }

        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (!ShouldDownloadNextPageVideos()) return;

                DownloadNextPage();
        }

        private bool ShouldDownloadNextPageVideos()
        {
            return !_isDownloading && _enhancedScroller.NormalizedScrollPosition <= LOADING_NEXT_PAGE_POSITION_THRESHOLD;
        }

        private void ShowNoFollowersText(bool show)
        {
            if (show)
            {
                _noFollowersText.text = ContextData.TabIndex == BaseFollowersPageArgs.FOLLOWERS_TAB_INDEX
                    ? "No followers yet but don't worry.\nJust keep creating!"
                    : "You're not following anyone yet.\nFind some Creators to follow!";
            }
            _noFollowersText.gameObject.SetActive(show); 
        }
    }
}

using System.Collections.Generic;
using Abstract;
using Bridge;
using Bridge.Services.UserProfile;
using EnhancedUI.EnhancedScroller;
using Extensions;
using Navigation.Core;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.FollowersPage.UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Home
{
    public class FriendsRowView : BaseContextDataView<long>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScrollerCellView _addFriendsItem;
        [SerializeField] private EnhancedScrollerCellView _userItemPrefab;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private int _maxAmount = 50;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private FollowersManager _followersManager;

        private Profile _userProfile;
        private readonly List<Profile> _profiles = new List<Profile>();
        
        private void Awake()
        {
            _addFriendsItem.GetComponent<Button>().onClick.AddListener(OnSearchFriendsClick);
        }

        protected override async void OnInitialized()
        {
            _userProfile = (await _bridge.GetProfile(ContextData)).Profile;
            var result = await _bridge.GetFriends(_userProfile.MainGroupId, _maxAmount, 0);
            
            if (_enhancedScroller.IsDestroyed()) return;
            
            _profiles.AddRange(result.Profiles);
            _enhancedScroller.Delegate = this;
            _enhancedScroller.ReloadData();
        }

        protected override void BeforeCleanup()
        {
            _enhancedScroller._RecycleAllCells();
            _profiles.Clear();
        }

        public int GetNumberOfCells(EnhancedScroller scroller) => _profiles.Count + 1;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _userItemPrefab.GetComponent<RectTransform>().GetWidth();
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == 0)
            {
                _addFriendsItem.gameObject.SetActive(true);
                return _addFriendsItem;
            }

            --dataIndex;

            var cellView = scroller.GetCellView(_userItemPrefab);
            var itemView = cellView.GetComponent<FriendsRowItemView>();

            itemView.Initialize(_profiles[dataIndex]);
            return cellView;
        }

        private void OnSearchFriendsClick()
        {
            _pageManager.MoveNext(PageId.FollowersPage, new RemoteUserFollowersPageArgs(_userProfile.MainGroupId, _followersManager, 0));
        }
    }
}
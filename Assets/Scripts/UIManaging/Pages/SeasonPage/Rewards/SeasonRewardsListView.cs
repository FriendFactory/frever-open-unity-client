using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using static UIManaging.Pages.SeasonPage.SeasonRewardsLevelView;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsListView : BaseContextDataView<SeasonRewardsListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _headerView;
        [SerializeField] private EnhancedScrollerCellView _purchasedPassView;
        [SerializeField] private EnhancedScrollerCellView _tabsView;
        [SerializeField] private EnhancedScrollerCellView _dividerView;
        [SerializeField] private EnhancedScrollerCellView _itemView;
        [SerializeField] private EnhancedScrollerCellView _bonusItemView;
        [SerializeField] private SeasonRewardsListViewAnimator _listViewAnimator;
        
        private float _headerCellHeight;
        private float _dividerCellHeight;
        private float _purchasedPassCellHeight;
        private float _tabsCellHeight;
        private float _bonusItemCellHeight;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _headerCellHeight = GetHeight(_headerView);
            _tabsCellHeight = GetHeight(_tabsView);
            _dividerCellHeight = GetHeight(_dividerView);
            _purchasedPassCellHeight = GetHeight(_purchasedPassView);
            _bonusItemCellHeight = GetHeight(_bonusItemView);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ReloadData()
        {
            if (IsDestroyed) return;
            _enhancedScroller.ReloadData();
        }

        public void RefreshActiveCards()
        {
            _enhancedScroller._RecycleAllCells();
            _enhancedScroller._Resize(true);
            _enhancedScroller._RefreshActive();
        }

        public void UpdateScrollPosition()
        {
            _listViewAnimator.ScrollDown();
        }
        
        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.Items.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            switch (ContextData.Items[dataIndex])
            {
                case SeasonRewardsPremiumPassModel model:
                    return _headerCellHeight;
                case SeasonRewardsPurchasedPremiumPassModel model:
                    return _purchasedPassCellHeight;
                case RewardTabsModel model:
                    return _tabsCellHeight;
                case SeasonRewardsDividerModel model:
                    return _dividerCellHeight;
                case SeasonRewardsLevelModel model:
                    return GetLevelCellSize(model);
                case SeasonRepeatableBonusesRewardsLevelModel model:
                    return _bonusItemCellHeight;
                default:
                    return 0;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellItem = ContextData.Items[dataIndex];
            switch (ContextData.Items[dataIndex])
            {
                case SeasonRewardsPremiumPassModel cellHeader:
                {
                    var cellView = scroller.GetCellView(_headerView);
                    var itemView = cellView.GetComponent<SeasonRewardsHeaderView>();
                    itemView.Initialize(cellHeader);
                    return cellView;
                }
                case RewardTabsModel tabsView:
                {
                    var cellView = scroller.GetCellView(_tabsView);
                    return cellView;
                }
                case SeasonRewardsDividerModel cellDivider:
                {
                    var cellView = scroller.GetCellView(_dividerView);
                    var itemView = cellView.GetComponent<SeasonRewardsDividerView>();
                    itemView.Initialize(cellDivider);
                    return cellView;
                }
                case SeasonRewardsLevelModel cellAsset:
                {
                    var cellView = scroller.GetCellView(_itemView);
                    var itemView = cellView.GetComponent<SeasonRewardsLevelView>();
                    itemView.Initialize(cellAsset);
                    return cellView;
                }
                case SeasonRewardsPurchasedPremiumPassModel model:
                {
                    var cellView = scroller.GetCellView(_purchasedPassView);
                    var component = cellView.GetComponent<SeasonRewardsPurchasedPassView>();
                    component.Initialize(model);
                    return cellView;
                }
                case SeasonRepeatableBonusesRewardsLevelModel model:
                {
                    var cellView = scroller.GetCellView(_bonusItemView);
                    var component = cellView.GetComponent<SeasonRepeatableBonusesRewardsLevelView>();
                    component.Initialize(model);
                    component.OnClaimedAllRepeatableBonusesRewards -= ClaimedAllRepeatableBonusesRewards;
                    component.OnClaimedAllRepeatableBonusesRewards += ClaimedAllRepeatableBonusesRewards;
                    return cellView;
                }
                default:
                {
                    Debug.LogWarning($"Cell type is not supported: {cellItem?.GetType()}");
                    return null;
                }
            }
        }

        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            ReloadData();
            
            _listViewAnimator.Initialize();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static float GetLevelCellSize(SeasonRewardsLevelModel model)
        {
            return DEFAULT_CELL_SIZE;
        }

        private static float GetHeight(MonoBehaviour component)
        {
            return component.GetComponent<RectTransform>().rect.height;
        }

        private void ClaimedAllRepeatableBonusesRewards(SeasonRepeatableBonusesRewardsLevelModel model)
        {
            if (!ContextData.Items.Contains(model)) return;
            ContextData.Items.Remove(model);
            ReloadData();
            UpdateScrollPosition();
        }
    }
}
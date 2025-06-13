using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Rewards.Models;
using UnityEngine;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal sealed class DiscoverPeoplePageContentList: BaseContextDataView<InvitationAcceptedRewardListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _headerView;
        [SerializeField] private EnhancedScrollerCellView _acceptedRewardView;

        private float _headerViewSize;
        private float _acceptedRewardViewSize;

        private void Awake()
        {
            _headerViewSize = _headerView.GetComponent<RectTransform>().rect.height;
            _acceptedRewardViewSize = _acceptedRewardView.GetComponent<RectTransform>().rect.height;
        }

        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData.Models.Count + 1;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) =>
            dataIndex == 0 ? _headerViewSize : _acceptedRewardViewSize;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == 0)
            {
                var headerCellView = scroller.GetCellView(_headerView);
                var sharePanel = headerCellView.GetComponentInChildren<InvitationLinkSharePanel>();

                if (!sharePanel.IsInitialized)
                {
                    sharePanel.Initialize(ContextData.InvitationCodeModel);
                }
                
                return headerCellView;
            }

            var cellView = scroller.GetCellView(_acceptedRewardView);
            var rewardModel = ContextData.Models[dataIndex - 1];
            var rewardView = cellView.GetComponent<InvitationAcceptedRewardItemView>();

            rewardModel.OnRewardClaimed += OnRewardClaimed;

            rewardView.Initialize(rewardModel);

            return cellView;
        }

        private void OnRewardClaimed(InvitationAcceptedRewardModel rewardModel)
        {
            rewardModel.OnRewardClaimed -= OnRewardClaimed;
            
            ContextData.RemoveReward(rewardModel);
            _scroller.ReloadData();
        }

        protected override void OnInitialized()
        {
            _scroller.Delegate = this;
        }

        protected override void BeforeCleanup()
        {
            _scroller.Delegate = null;
        }
    }
}
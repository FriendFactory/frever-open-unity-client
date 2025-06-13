using Abstract;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewMembersListView : BaseContextDataView<CrewMembersListModel>, IEnhancedScrollerDelegate
    {
        private const float CELL_WIDTH = 180.0f;
        
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _crewMemberView;
        [SerializeField] private EnhancedScrollerCellView _blockedMemberView;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _scroller.Delegate = this;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ReloadData()
        {
            if (IsDestroyed) return;
            _scroller.ReloadData();
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (ContextData is null) return 0;
            
            return ContextData.BlockedMembers + ContextData.MemberModels.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return CELL_WIDTH;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex < ContextData.BlockedMembers) return HandleBlockedUser();
            
            var cellView = scroller.GetCellView(_crewMemberView);
            var memberView = cellView.GetComponent<CrewMemberView>();
            var model = ContextData.MemberModels[dataIndex - ContextData.BlockedMembers];
            memberView.Initialize(model);

            return cellView;
        }

        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            ReloadData();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            if (ContextData == null || ContextData.MemberModels.IsNullOrEmpty()) return;
            
            foreach (var memberModel in ContextData.MemberModels)
            {
                if (memberModel.Portrait == null) continue;
                Destroy(memberModel.Portrait);
                memberModel.SetPortraitTexture(null);;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private EnhancedScrollerCellView HandleBlockedUser()
        {
            var cellView = _scroller.GetCellView(_blockedMemberView);

            return cellView;
        }
    }
}
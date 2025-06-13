using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Common.SearchPanel;
using UnityEngine;

namespace UIManaging.Pages.DiscoverPeoplePage.StarCreator.AcceptedInvitations
{
    internal sealed class AcceptedInvitationsList: BaseContextDataView<AcceptedInvitationsListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _profileView;
        [SerializeField] private GameObject _noAcceptedInvitationsText;

        private float _cellViewSize;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _noAcceptedInvitationsText.gameObject.SetActive(false);
        }
        
        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate implementation
        //---------------------------------------------------------------------

        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData.Profiles.Count;
        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => _cellViewSize;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_profileView);
            
            var itemView = cellView.GetComponent<SearchUserBaseItemView>();
            itemView.Initialize(ContextData.Profiles[dataIndex]);
            
            return cellView;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            if (ContextData.Profiles.Count == 0)
            {
                _noAcceptedInvitationsText.SetActive(true);
                return;
            }
            
            _enhancedScroller.Delegate = this;
            _cellViewSize = _profileView.GetComponent<RectTransform>().rect.height;
        }

        protected override void BeforeCleanup()
        {
            _enhancedScroller.ClearAll();
            _enhancedScroller.Delegate = null;
        }
    }
}
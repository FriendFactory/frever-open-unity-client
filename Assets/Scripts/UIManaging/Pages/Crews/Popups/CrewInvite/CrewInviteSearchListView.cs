using System;
using System.Linq;
using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Pages.Crews.Popups;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    [RequireComponent(typeof(EnhancedScroller))]
    public class CrewInviteSearchListView : BaseContextDataView<CrewInviteSearchListModel>, IEnhancedScrollerDelegate
    {
        public event Action OnScrolledToLastScreen;
        
        [SerializeField] private GameObject _noResultPanel;
        [SerializeField] private EnhancedScrollerCellView _friendView;

        private EnhancedScroller _enhancedScroller;

        private void Awake()
        {
            _enhancedScroller = GetComponent<EnhancedScroller>();
            _enhancedScroller.Delegate = this;
            _enhancedScroller.scrollerScrolled += OnScrolled;
        }

        private void OnDisable()
        {
            OnScrolledToLastScreen = null;
            _enhancedScroller.ScrollPosition = 0.0f;
            _enhancedScroller._RecycleAllCells();
        }

        public void Reload()
        {
            if (ContextData.Friends == null || ContextData.Friends.Count == 0)
            {
                _enhancedScroller.ClearActive();
                return;
            }
            
            _enhancedScroller._Resize(true);
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (ContextData is null || ContextData.Friends is null) return 0;

            return ContextData.Friends.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 195.0f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = _enhancedScroller.GetCellView(_friendView);

            var itemView = cellView.GetComponent<CrewInviteListItem>();
            var data = ContextData.Friends[dataIndex];
            var isInvited = ContextData.InvitedUsers.Contains(data.MainGroupId);
            var model = new CrewInviteListItemModel(data.MainGroupId, data.NickName, data.KPI.FollowersCount, isInvited, ContextData.Token);
            itemView.Initialize(model);
            
            return cellView;
        }
        
        private void OnScrolled(EnhancedScroller scroller, Vector2 val, float scrollposition)
        {
            if (ContextData is null || _enhancedScroller.NumberOfCells == 0)
            {
                OnScrolledToLastScreen?.Invoke();
                return;
            }
            
            var scrolledToNextPage = ContextData.Friends.Count - _enhancedScroller.EndDataIndex < 5;
            if (!scrolledToNextPage) return;
            OnScrolledToLastScreen?.Invoke();
        }

        protected override void OnInitialized() { }
    }
}
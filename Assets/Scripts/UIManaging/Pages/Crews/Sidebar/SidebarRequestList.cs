using Abstract;
using Bridge;
using EnhancedUI.EnhancedScroller;
using Modules.Crew;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarRequestList : BaseContextDataView<SidebarRequestListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _requestView;

        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private CrewService _crewService;

        private void Awake()
        {
            _scroller.Delegate = this;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.RequestModels?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 96.0f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cell = _scroller.GetCellView(_requestView);
            var view = cell.GetComponent<SidebarRequestListItem>();
            view.Initialize(ContextData.RequestModels[dataIndex]);
            view.AcceptActionRequested = AcceptAction;
            view.RejectActionRequested = RejectAction;
            return cell;
        }

        protected override void OnInitialized()
        {
            _scroller.ReloadData();
        }

        private void AcceptAction(int dataIndex, long groupId, long requestId)
        {
            var username = ContextData.RequestModels[dataIndex].Username;
            ContextData.RequestModels.RemoveAt(dataIndex);
            _crewService.AcceptRequest(groupId, username, requestId);
            _scroller.ReloadData();
        }

        private void RejectAction(int dataIndex, long groupId, long requestId)
        {
            var username = ContextData.RequestModels[dataIndex].Username;
            ContextData.RequestModels.RemoveAt(dataIndex);
            _scroller.ReloadData();
            _crewService.RejectRequest(groupId, username, requestId);
        }
    }
}
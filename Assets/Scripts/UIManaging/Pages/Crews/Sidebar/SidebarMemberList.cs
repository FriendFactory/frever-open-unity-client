using System;
using Abstract;
using Bridge;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMemberList : BaseContextDataView<SidebarMembersModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _headerPrefab;
        [SerializeField] private EnhancedScrollerCellView _rolePrefab;
        [SerializeField] private EnhancedScrollerCellView _memberPrefab;

        protected override void OnInitialized()
        {
            _scroller.Delegate = this;
        }

        public void Refresh()
        {
            _scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData.Models?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            switch (ContextData.Models[dataIndex])
            {
                case SidebarMembersListHeaderModel header:
                    return 102f;
                case SidebarMembersListRoleModel role:
                    return 51f;
                case SidebarMembersListMemberModel member:
                    return 90f;
            }

            return 0f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var model = ContextData.Models[dataIndex];

            switch (model)
            {
                case SidebarMembersListHeaderModel header:
                {
                    var cellView = scroller.GetCellView(_headerPrefab);
                    var headerView = cellView.GetComponent<SidebarMembersListHeaderView>();
                    headerView.Initialize(header);

                    return cellView;
                }

                case SidebarMembersListRoleModel role:
                {
                    var cellView = scroller.GetCellView(_rolePrefab);
                    var roleView = cellView.GetComponent<SidebarMembersListRoleView>();
                    roleView.Initialize(role);

                    return cellView;
                }

                case SidebarMembersListMemberModel member:
                {
                    var cellView = scroller.GetCellView(_memberPrefab);
                    var memberView = cellView.GetComponent<SidebarMembersListMemberView>();
                    memberView.Initialize(member);

                    return cellView;
                }

                default:
                    throw new ArgumentOutOfRangeException($"Unknown element type {model.GetType()}");
            }
        }
    }
}
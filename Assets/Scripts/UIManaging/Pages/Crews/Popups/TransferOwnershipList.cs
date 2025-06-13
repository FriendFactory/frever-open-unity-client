using System;
using System.Collections.Generic;
using System.Linq;
using Abstract;
using Bridge.Models.ClientServer.Crews;
using EnhancedUI.EnhancedScroller;
using Modules.Crew;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class TransferOwnershipListModel
    {
        public List<TransferOwnershipListItemModel> Members;

        public TransferOwnershipListModel(CrewMember[] members, long localGroupId, CrewPageLocalization localization)
        {
            var validMembers = members
                              .Where(m => m.Group.Id != localGroupId)
                              .OrderBy(m => m.RoleId);

            Members = new List<TransferOwnershipListItemModel>();
            foreach (var member in validMembers)
            {
                var roleName = localization.GetRankLocalized(member.RoleId);
                var model = new TransferOwnershipListItemModel(member.Group.Id, member.Group.Nickname, roleName,
                                                               member.IsOnline);
                
                Members.Add(model);
            }
        }
    }

    
    internal sealed class TransferOwnershipList : BaseContextDataView<TransferOwnershipListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _item;
        [SerializeField] private ToggleGroup _toggleGroup;

        public event Action<long> NewLeaderSelected;

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }

        private void OnDisable()
        {
            _enhancedScroller.ClearAll();
        }

        protected override void OnInitialized()
        {
            _enhancedScroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return (int)ContextData?.Members?.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 150.0f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var model = ContextData.Members[dataIndex];
            model.ToggleGroup = _toggleGroup;
            
            var cell = _enhancedScroller.GetCellView(_item);
            var view = cell.GetComponent<TransferOwnershipListItem>();
            view.OnToggleValueChanged += OnToggleValueChanged;
            view.Initialize(model);

            return cell;
        }

        private void OnToggleValueChanged(long groupId)
        {
            NewLeaderSelected?.Invoke(groupId);
        }
    }
}
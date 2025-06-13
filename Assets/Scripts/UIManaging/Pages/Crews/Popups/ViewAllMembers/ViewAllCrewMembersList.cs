using System;
using System.Collections.Generic;
using System.Threading;
using Abstract;
using Bridge;
using EnhancedUI.EnhancedScroller;
using Extensions;
using Modules.Crew;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Crews.Popups;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    [RequireComponent(typeof(EnhancedScroller))]
    internal sealed class ViewAllCrewMembersList : BaseContextDataView<ViewAllCrewMembersListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScrollerCellView _memberItem;
        [SerializeField] private EnhancedScrollerCellView _blockedMember;

        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private CrewService _crewService;
        
        private EnhancedScroller _enhancedScroller;
        private List<ViewAllCrewMembersListItemModel> _itemModels;
        
        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            _enhancedScroller = GetComponent<EnhancedScroller>();
            _enhancedScroller.Delegate = this;
        }

        private void OnEnable()
        {
            if (_itemModels is null) return;
            
            FetchScoreList();
        }

        private void OnDisable()
        {
            if (_itemModels is null) return;
            
            _enhancedScroller.ClearActive();
            _tokenSource.CancelAndDispose();
            _tokenSource = null;
            _itemModels = null;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData.BlockedMembers + (int)ContextData?.MembersCount;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 120.0f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex >= ContextData.MembersCount) return HandleBlockedMember();
            
            var cellView = _enhancedScroller.GetCellView(_memberItem);
            var itemView = cellView.GetComponent<ViewAllCrewMembersListItem>();

            itemView.Initialize(GetListItemModel(dataIndex));
            return cellView;
        }
        
        protected override void OnInitialized()
        {
            _itemModels = new List<ViewAllCrewMembersListItemModel>();
            for (var i = 0; i < ContextData.MembersCount; i++)
            {
                var model = new ViewAllCrewMembersListItemModel(i + 1, ContextData.LocalUserGroupId);
                _itemModels.Add(model);
            }
            _enhancedScroller.ReloadData();

            _tokenSource = new CancellationTokenSource();
        }

        private ViewAllCrewMembersListItemModel GetListItemModel(int dataIndex)
        {
            var model = _itemModels[dataIndex];
            
            if (!(model is null)) return model;
            
            model = new ViewAllCrewMembersListItemModel(dataIndex + 1, ContextData.LocalUserGroupId);
            _itemModels[dataIndex] = model;

            return model;
        }

        private async void FetchScoreList()
        {
            var result = await _bridge.GetCrewMembersTopList(ContextData.CrewId, _tokenSource.Token);
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);
            if (!result.IsSuccess) return;
            
            var models = result.Models;
            var memberDataOutOfSync = result.Models.Length != ContextData.MembersCount;
            for (var i = 0; i < models.Length; i++)
            {
                // If new members joined since last time crew data was refreshed.
                if (i == _itemModels.Count)
                {
                    var model = new ViewAllCrewMembersListItemModel(i, models[i].Group.Id);
                    model.Update(models[i].Group.Id, models[i].IsOnline, models[i].Group.Nickname,
                                 models[i].Trophies.ToString());
                    _itemModels.Add(model);

                    continue;
                }

                _itemModels[i].Update(models[i].Group.Id, models[i].IsOnline, models[i].Group.Nickname,
                                      models[i].Trophies.ToString());
            }

            if (memberDataOutOfSync) HandleMemberDataOutOfSync(models.Length);
        }

        private void HandleMemberDataOutOfSync(int membersCount)
        {
            ContextData.MembersCount = membersCount;
            _enhancedScroller.ReloadData();
            
            // refresh crew data to make rest of the crew page up to date (this will also trigger refresh on pages
            // beneath the overlay)
            _crewService.RefreshCrewData();
        }

        private EnhancedScrollerCellView HandleBlockedMember()
        {
            var cell = _enhancedScroller.GetCellView(_blockedMember);
            return cell;
        }
    }
}
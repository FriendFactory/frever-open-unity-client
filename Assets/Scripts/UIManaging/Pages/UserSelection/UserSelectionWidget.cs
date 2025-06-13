using System.Collections.Generic;
using Abstract;
using UIManaging.Common.SearchPanel;
using UnityEngine;

namespace UIManaging.Pages.UserSelection
{
    public class UserSelectionWidget : BaseContextDataView<UserSelectionPanelModel>
    {
        [SerializeField] private SearchHandler _searchHandler;
        [SerializeField] private SearchPanelView _searchPanelView;
        [SerializeField] private UserSelectionDataHolder _userSelectionDataHolder;
        [SerializeField] private UserSelectionPanelView _userSelectionPanelView;
        public IReadOnlyList<UserSelectionItemModel> SelectedUsers => _userSelectionDataHolder.ContextData.SelectedItems;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _userSelectionDataHolder.Initialize(ContextData);
            _userSelectionPanelView.Initialize(ContextData);

            _searchHandler.FilterGroupIds = ContextData.FilterGroupIds;
            
            if (ContextData.TargetProfileId.HasValue)
            {
                _searchHandler.SetTargetProfile(ContextData.TargetProfileId.Value);
            }
            else
            {
                _searchHandler.SetTargetProfileToLocalUser();
            }
            
            _searchHandler.SetUsersFilter(ContextData.Filter, true);
            _searchHandler.SetSearchHandling(true);
        }

        protected override void BeforeCleanup()
        {
            _searchPanelView.Clear();
            
            _searchHandler.SetSearchHandling(false);

            ContextData?.Clear();

            _userSelectionPanelView.CleanUp();
            _userSelectionDataHolder.CleanUp();
        }
    }
}
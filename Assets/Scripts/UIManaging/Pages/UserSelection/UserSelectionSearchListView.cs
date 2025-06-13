using System.Linq;
using Bridge.Models.ClientServer;
using EnhancedUI.EnhancedScroller;
using UIManaging.Common.SearchPanel;
using UIManaging.Common.SelectionPanel;
using UnityEngine;

namespace UIManaging.Pages.UserSelection
{
    public class UserSelectionSearchListView : SearchListView
    {
        [SerializeField] private UserSelectionDataHolder _selectionDataHolder;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _selectionDataHolder.ContextData?.AddItems(ContextData.Users.Select(profile => new GroupShortInfo
            {
                Id = profile.MainGroupId,
                Nickname = profile.NickName,
                MainCharacterId = profile.MainCharacter.Id,
                MainCharacterFiles = profile.MainCharacter.Files
            }).ToArray());
        }
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = base.GetCellView(scroller, dataIndex, cellIndex);
            
            var itemView = cellView.GetComponent<SelectionCheckmarkView>();
            itemView.Initialize(_selectionDataHolder.GetSelectionItem(ContextData.Users[dataIndex]));
            
            return cellView;
        }
    }
}
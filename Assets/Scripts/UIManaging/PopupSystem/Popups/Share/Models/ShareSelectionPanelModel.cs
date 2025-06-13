using System;
using System.Collections.Generic;
using UIManaging.Common.SelectionPanel;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups.Share
{
    public sealed class ShareSelectionPanelModel: SelectionPanelModel<ShareSelectionItemModel>
    {
        public string ConfirmButtonText;
        public Action<ShareDestinationData> OnConfirmed;
        public readonly bool BlockConfirmButtonIfNoSelection;

        public ShareSelectionPanelModel(int maxSelected,
            ICollection<ShareSelectionItemModel> preselectedItems,
            ICollection<ShareSelectionItemModel> lockedItems, string confirmButtonText,
            Action<ShareDestinationData> onConfirmed, bool blockConfirmButtonIfNoSelection) :
            base(maxSelected, preselectedItems, lockedItems)
        {
            ConfirmButtonText = confirmButtonText;
            OnConfirmed = onConfirmed;
            BlockConfirmButtonIfNoSelection = blockConfirmButtonIfNoSelection;
        }
        
        public void ClearNonSelected()
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                var item = _items[i];

                if (item.IsSelected) continue;
                
                _items.RemoveAt(i);
                _actions.Remove(item);
            }
        }
    }
}
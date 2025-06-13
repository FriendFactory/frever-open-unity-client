using System;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using UIManaging.Common.SelectionPanel;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionPanel : SelectionPanelView<ShareSelectionPanelModel, ShareSelectionItemModel>
    {
        [SerializeField] private EnhancedScrollerCellView _friendsSelectedItem;
        [SerializeField] private EnhancedScrollerCellView _groupChatsSelectedItem;
        [SerializeField] private EnhancedScrollerCellView _directChatsSelectedItem;
        [SerializeField] private EnhancedScrollerCellView _crewChatsSelectedItem;
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var model = ContextData.SelectedItems[dataIndex];

            switch (model)
            {
                case ShareSelectionFriendsItemModel friendsItemModel:
                    return GetFriendsCellView(friendsItemModel);
                case ShareSelectionChatsItemModel chatsItemModel:
                    return GetChatsCellView(chatsItemModel);
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EnhancedScrollerCellView GetFriendsCellView(ShareSelectionFriendsItemModel friendsItemModel)
            {
                var cellView = scroller.GetCellView(_friendsSelectedItem);
                var friendsItem = cellView.GetComponent<ShareSelectionFriendsSelectedItem>();
                
                if (!friendsItem.IsInitialized || friendsItem.ContextData.Id != friendsItemModel.Id)
                {
                    friendsItem.Initialize(friendsItemModel);
                }

                return cellView;
            }

            EnhancedScrollerCellView GetChatsCellView(ShareSelectionChatsItemModel chatsItemModel)
            {
                var cellView = GetChatCellView(chatsItemModel);
                var chatsItem = cellView.GetComponent<ShareSelectionChatsSelectedItem>();

                if (!chatsItem.IsInitialized || chatsItem.ContextData.Id != chatsItemModel.Id)
                {
                    chatsItem.Initialize(chatsItemModel);
                }

                return cellView;

                EnhancedScrollerCellView GetChatCellView(ShareSelectionItemModel chatModel)
                {
                    switch (chatModel)
                    {
                        case ShareSelectionDirectChatsItemModel _:
                            return scroller.GetCellView(_directChatsSelectedItem);
                        case ShareSelectionGroupChatsItemModel _:
                            return scroller.GetCellView(_groupChatsSelectedItem);
                        case ShareSelectionCrewChatsItemModel _:
                            return scroller.GetCellView(_crewChatsSelectedItem);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(chatModel));
                    }
                }
            }
        }
        
        public void ReloadData()
        {
            EnhancedScroller.ReloadData();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (ContextData.SelectedItems.Any())
            {
                Resize();
            }
        }

        protected override void BeforeCleanup()
        {
            EnhancedScroller.ClearAll();
            
            base.BeforeCleanup();
        }
    }
}
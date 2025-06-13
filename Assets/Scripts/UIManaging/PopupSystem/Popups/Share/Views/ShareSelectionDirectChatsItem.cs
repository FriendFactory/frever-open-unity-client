using System.Linq;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionDirectChatsItem: ShareSelectionChatsItem
    {
        [SerializeField] private UserPortraitView _userPortraitView;

        protected override void RefreshChatsPortraitImage()
        {
            var user = ContextData.ChatShortInfo.Members.FirstOrDefault(userFromList => userFromList.Id != DataHolder.GroupId);
            
            _userPortraitView.InitializeFromGroupInfo(user);
        }
    }
}
using System.Linq;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class ShareSelectionDirectChatsSelectedItem: ShareSelectionChatsSelectedItem
    {
        [SerializeField] private UserPortraitView _userPortraitView;
        
        [Inject] private LocalUserDataHolder _dataHolder;

        protected override void RefreshPortraitImage()
        {
            var user = ContextData.ChatShortInfo.Members
                                  .FirstOrDefault(userFromList => userFromList.Id != _dataHolder.GroupId);
            
            _userPortraitView.InitializeFromGroupInfo(user);
        }
        
        protected override void BeforeCleanup()
        {
            _userPortraitView.CleanUp();
            
            base.BeforeCleanup();
        }
    }
}
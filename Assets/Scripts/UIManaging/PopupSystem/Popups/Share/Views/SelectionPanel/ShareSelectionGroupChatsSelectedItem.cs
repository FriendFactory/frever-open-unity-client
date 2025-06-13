using System.Collections.Generic;
using System.Linq;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class ShareSelectionGroupChatsSelectedItem: ShareSelectionChatsSelectedItem
    {
        [SerializeField] private List<UserPortraitView> _groupPortraitViews;
        
        [Inject] private LocalUserDataHolder _dataHolder;
        
        protected override void RefreshPortraitImage()
        {
            var users = ContextData.ChatShortInfo.Members
                       .Where(userFromList => userFromList.Id != _dataHolder.GroupId && userFromList.MainCharacterId.HasValue)
                       .Take(_groupPortraitViews.Count)
                       .ToArray();

            if (users.Length < 2)
            {
                Debug.LogError($"Insufficient users with valid portraits in group chat {ContextData.Id}");
                return;
            }
            
            for (var i = 0; i < _groupPortraitViews.Count; i++)
            {
                var userPortraitView = _groupPortraitViews[i];
                var user = users[i];
                
                userPortraitView.InitializeFromGroupInfo(user);
            }
        }
        
        protected override void BeforeCleanup()
        {
            _groupPortraitViews.ForEach(view => view.CleanUp());
            
            base.BeforeCleanup();
        }
    }
}
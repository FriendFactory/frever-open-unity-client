using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionGroupChatsItem: ShareSelectionChatsItem
    {
        [SerializeField] private TMP_Text _usersCounter;
        [SerializeField] private List<UserPortraitView> _groupPortraitViews;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _usersCounter.text = ContextData.ChatShortInfo.Members.Length.ToString();
        }

        protected override void RefreshChatsPortraitImage()
        {
            var users = ContextData.ChatShortInfo.Members
                       .Where(userFromList => userFromList.Id != DataHolder.GroupId && userFromList.MainCharacterId.HasValue)
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
    }
}
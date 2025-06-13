using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.InboxPage.Views
{
    public class GroupChatItemView: ChatItemViewBase
    {
        [SerializeField] private List<UserPortraitView> _userPortraitViews;
        [SerializeField] private TextMeshProUGUI _userCounterText;

        [Inject] private LocalUserDataHolder _dataHolder;

        protected override void UpdateData()
        {
            base.UpdateData();

            _userCounterText.text = ContextData.Users.Count.ToString();
            
            var users = ContextData.Users
                                   .Where(userFromList => userFromList.Id != _dataHolder.GroupId && userFromList.MainCharacterId.HasValue)
                                   .ToArray();

            for (var i = 0; i < _userPortraitViews.Count; i++)
            {
                SetUserThumbnail(_userPortraitViews[i], i < users.Length ? users[i] : null);
            }
        }
    }
}
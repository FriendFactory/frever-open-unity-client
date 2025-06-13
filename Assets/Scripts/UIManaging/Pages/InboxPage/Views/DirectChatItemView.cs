using System.Linq;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.InboxPage.Views
{
    public class DirectChatItemView: ChatItemViewBase
    {
        [SerializeField] private UserPortraitView _userPortraitView;

        [Inject] private LocalUserDataHolder _dataHolder;

        protected override void UpdateData()
        {
            base.UpdateData();

            var user = ContextData.Users.FirstOrDefault(userFromList => userFromList.Id != _dataHolder.GroupId);

            SetUserThumbnail(_userPortraitView, user);
        }
    }
}
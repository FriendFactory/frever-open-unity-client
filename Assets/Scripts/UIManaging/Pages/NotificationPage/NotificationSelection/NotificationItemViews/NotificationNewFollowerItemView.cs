using Bridge.Services.UserProfile;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Common.Args.Buttons;
using UIManaging.Pages.FollowersPage.UI;
using UnityEngine;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationNewFollowerItemView : UserBasedNotificationItemView<NotificationNewFollowerItemModel>
    {
        [SerializeField] private FollowUserButton _followButton;
        protected override string Description => _localization.NewFollowerFormat;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (UserProfile == null || ContextData.GroupId != UserProfile.MainGroupId)
            {
                SetupInitialFollowButtonState();
            }
            else
            {
                SetupFollowButtonWithRefreshedData();
            }
        }

        protected override void OnSetupComplete()
        {
            SetupFollowButtonWithRefreshedData();
        }

        private void SetupInitialFollowButtonState()
        {
            var profile = new Profile
            {
                MainGroupId = ContextData.GroupId,
                UserFollowsYou = true,
                YouFollowUser = ContextData.AreFriends,
                NickName = ContextData.UserNickname
            };
            profile.YouFollowUser = ContextData.AreFriends ||
                                    FollowersManager.LocalUserFollowed.ExistInCachedList(profile.MainGroupId);
            _followButton.Initialize(new FollowUserButtonArgs(profile));
        }

        private void SetupFollowButtonWithRefreshedData()
        {
            _followButton.Initialize(new FollowUserButtonArgs(UserProfile));
        }
    }
}
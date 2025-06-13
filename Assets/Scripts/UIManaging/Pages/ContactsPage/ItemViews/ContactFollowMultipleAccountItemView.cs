using UIManaging.Common.Args.Buttons;
using UIManaging.Pages.FollowersPage.UI;
using UnityEngine;

namespace UIManaging.Pages.ContactsPage.ItemViews
{
    internal sealed class ContactFollowMultipleAccountItemView : ContactWithAccountItemView
    {
        [SerializeField] private MultipleFollowUserButton followUserButton;

        protected override void OnInitialized()
        {
            AllowGoToUserProfile = false;
            base.OnInitialized();
        }

        protected override void OnUserProfileDownloaded()
        {
            base.OnUserProfileDownloaded();
            
            followUserButton.Initialize(new MultipleFollowUserButtonArgs(UserProfile, ContextData.OnFollow, UnfollowUser));
        }
        
        private void UnfollowUser(long userId)
        {
            ContextData.IsFollowed = false;
            ContextData.OnUnfollow?.Invoke(userId);
        }

    }
}

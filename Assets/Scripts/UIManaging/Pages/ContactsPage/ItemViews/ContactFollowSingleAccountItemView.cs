using UIManaging.Common.Args.Buttons;
using UIManaging.Pages.FollowersPage.UI;
using UnityEngine;

namespace UIManaging.Pages.ContactsPage.ItemViews
{
    internal sealed class ContactFollowSingleAccountItemView : ContactWithAccountItemView
    {
        [SerializeField] private FollowUserButton _followButton;
        
        private void OnEnable()
        {
            RefreshFollowButton();
        }

        protected override void OnInitialized()
        {
            _followButton.gameObject.SetActive(false);
            base.OnInitialized();
        }

        protected override void OnUserProfileDownloaded()
        {
            _followButton.gameObject.SetActive(true);
            _followButton.Initialize(new FollowUserButtonArgs(UserProfile));
            base.OnUserProfileDownloaded();
        }
        
        private void RefreshFollowButton()
        {
            if (UserProfile == null) return;
            _followButton.Initialize(new FollowUserButtonArgs(UserProfile));
        }
    }
}

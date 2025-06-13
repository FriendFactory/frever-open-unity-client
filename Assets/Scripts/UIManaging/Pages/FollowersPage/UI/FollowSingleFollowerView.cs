using UIManaging.Common.Args.Buttons;
using UnityEngine;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowSingleFollowerView : FollowerView
    {
        [SerializeField] private FollowUserButton _followUserButton;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetupFollowButton();
        }

        private void SetupFollowButton()
        {
            _followUserButton.Initialize(new FollowUserButtonArgs(ContextData.UserProfile));
        }
    }
}

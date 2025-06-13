namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowUserVideoDescriptionButton : FollowUserButton
    {
        protected override string FollowButtonText => string.Format(_localization.FollowUsernameButtonFormat, ContextData.Profile.NickName);
        protected override string FollowBackButtonText => string.Format(_localization.FollowBackUsernameButtonFormat, ContextData.Profile.NickName);

        protected override void OnUnfollowButtonClicked() => OnUnfollowButtonClickedInternal();
    }
}
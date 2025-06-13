namespace UIManaging.Pages.FollowersPage.UI.FollowersLists
{
    public class FollowersListModel
    {
        public FollowerViewModel[] Followers { get; }

        public FollowersListModel(FollowerViewModel[] followers)
        {
            Followers = followers;
        }
    }
}
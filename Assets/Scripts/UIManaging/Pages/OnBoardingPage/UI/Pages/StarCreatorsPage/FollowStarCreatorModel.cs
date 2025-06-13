using UIManaging.Common.Args.Views.Profile;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal sealed class FollowStarCreatorModel
    {
        public readonly long GroupId;
        public readonly string Nickname;
        public readonly int Followers;
        public readonly UserPortraitModel PortraitModel;

        public FollowStarCreatorModel(long groupId, string nickname, int followers, UserPortraitModel portraitModel)
        {
            GroupId = groupId;
            Nickname = nickname;
            Followers = followers;
            PortraitModel = portraitModel;
            IsMarkedToFollow = true;
        }
        
        public bool IsMarkedToFollow { get; private set; }

        public void MarkToFollow(bool value)
        {
            IsMarkedToFollow = value;
        }
    }
}
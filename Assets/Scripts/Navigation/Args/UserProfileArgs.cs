using Navigation.Core;

namespace Navigation.Args
{
    public class UserProfileArgs : PageArgs
    {
        public override PageId TargetPage => PageId.UserProfile;
        public long GroupId { get; protected set; }
        public bool ShowBackButton { get; private set; }
        public bool ShowNavigationBar { get; private set; }
        public string NicknameOverride { get; protected set; }

        public UserProfileArgs(bool showBackButton = false, bool showNavigationBar = true)
        {
            GroupId = 0;
            ShowBackButton = showBackButton;
            ShowNavigationBar = showNavigationBar;
        }
        
        public UserProfileArgs(long groupId, string nickname, bool showBackButton = true, bool showNavigationBar = false)
        {
            GroupId = groupId;
            ShowBackButton = showBackButton;
            ShowNavigationBar = showNavigationBar;
            NicknameOverride = nickname;
        }
    }
}

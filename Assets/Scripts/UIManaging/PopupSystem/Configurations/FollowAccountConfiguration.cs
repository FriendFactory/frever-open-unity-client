using Bridge.Services.UserProfile;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class FollowAccountConfiguration: PopupConfiguration
    {
        public Profile Profile;

        public FollowAccountConfiguration():base(PopupType.FollowAccountPopup, null, null)
        {
        }
    }
}
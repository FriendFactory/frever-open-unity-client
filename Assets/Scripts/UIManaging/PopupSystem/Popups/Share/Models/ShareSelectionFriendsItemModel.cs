using Bridge.Services.UserProfile;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionFriendsItemModel : ShareSelectionItemModel 
    {
        public ShareSelectionFriendsItemModel(Profile  profile, bool isSelected = false): base(isSelected)
        {
            Profile = profile;
        }

        public Profile Profile { get; }
        
        public override long Id => Profile.MainGroupId;
        public override string Title => Profile?.NickName;
    }
}
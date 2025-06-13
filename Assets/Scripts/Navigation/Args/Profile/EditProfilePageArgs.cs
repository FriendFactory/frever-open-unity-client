using Bridge.Services.UserProfile;
using Navigation.Core;

namespace Navigation.Args
{
    public class EditProfilePageArgs : PageArgs
    {
        public EditProfilePageArgs(Profile profile)
        {
            Profile = profile;
        }

        public Profile Profile { get; }
        public override PageId TargetPage => PageId.EditProfile;
    }
}
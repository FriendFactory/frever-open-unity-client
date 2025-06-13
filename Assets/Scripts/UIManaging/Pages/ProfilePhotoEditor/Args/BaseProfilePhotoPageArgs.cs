using Bridge.Services.UserProfile;
using Modules.PhotoBooth.Profile;
using Navigation.Core;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    public abstract class BaseProfilePhotoPageArgs : PageArgs
    {
        public Profile Profile { get; set; }
        public ProfilePhotoType PhotoType { get; set; }
        public PageId OnConfirmBackPageId { get; set; }
    }
}
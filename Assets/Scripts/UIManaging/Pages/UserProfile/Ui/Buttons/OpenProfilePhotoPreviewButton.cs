using UIManaging.Pages.UserProfile.Ui.ProfileHelper;
using UnityEngine;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    internal class OpenProfilePhotoPreviewButton : OpenProfilePhotoPreviewButtonBase
    {
        [SerializeField] private UserProfilePage _userProfilePage;

        protected override BaseUserProfileHelper UserProfileHelper => _userProfilePage.UserProfileHelper;
    }
}
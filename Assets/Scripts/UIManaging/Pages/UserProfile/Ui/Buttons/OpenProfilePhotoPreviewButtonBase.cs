using Modules.PhotoBooth.Profile;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.Pages.ProfilePhotoEditing;
using UIManaging.Pages.UserProfile.Ui.ProfileHelper;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    internal abstract class OpenProfilePhotoPreviewButtonBase: ButtonBase
    {
        [SerializeField] private ProfilePhotoType _photoType;
        [SerializeField] private RawImage _portraitImage;
        
        protected abstract BaseUserProfileHelper UserProfileHelper { get; }

        protected override void OnClick()
        {
            // spam tapping guard
            if (!_portraitImage.texture) return;
            
            // TODO: find a way to reuse downloaded thumbnail 
            var sourceTexture = _portraitImage.texture;
            var texture = new Texture2D(sourceTexture.width, sourceTexture.height);
            Graphics.CopyTexture(sourceTexture, 0, 0, texture, 0, 0);

            var args = new ProfilePhotoPreviewPageArgs
            {
                Profile = UserProfileHelper.IsCurrentUser ? UserProfileHelper.Profile : null,
                PhotoType = _photoType,
                Photo = texture,
            };

            Manager.MoveNext(PageId.ProfilePhotoPreview, args);
        }
    }
}
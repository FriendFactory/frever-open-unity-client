using UnityEngine;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    public abstract class BaseProfilePhotoPreviewPageArgs: BaseProfilePhotoPageArgs
    {
        public Texture2D Photo { get; set; }
    }
}
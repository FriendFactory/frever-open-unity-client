using System.Threading;
using Modules.PhotoBooth.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    internal class PhotoPreviewController: MonoBehaviour
    {
        [SerializeField] private MaskablePhotoPreview _profilePhotoPreview;
        [SerializeField] private MaskablePhotoPreview _backgroundPhotoPreview;
        
        private Texture _photo;
        private RawImage _targetImage;

        public void Initialize(BaseProfilePhotoPreviewPageArgs args)
        {
            _profilePhotoPreview.gameObject.SetActive(false);
            _backgroundPhotoPreview.gameObject.SetActive(false);

            var photoPreview = args.PhotoType == ProfilePhotoType.Profile
                ? _profilePhotoPreview
                : _backgroundPhotoPreview;
            
            InitializePhotoPreview(photoPreview, args);
        }

        public void CleanUp()
        {
            _profilePhotoPreview.CleanUp();
            _backgroundPhotoPreview.CleanUp();
        }

        private void InitializePhotoPreview(MaskablePhotoPreview photoPreview, BaseProfilePhotoPreviewPageArgs args)
        {
            photoPreview.gameObject.SetActive(true);
            photoPreview.InitializeAsync(args, CancellationToken.None);
            photoPreview.ShowContent();
        }
    }
}
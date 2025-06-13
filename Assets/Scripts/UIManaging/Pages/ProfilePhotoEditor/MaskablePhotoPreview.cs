using System.Threading;
using Common.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.ProfilePhotoEditing
{
    public class MaskablePhotoPreview: UIElementWithPlaceholder<BaseProfilePhotoPreviewPageArgs>
    {
        [SerializeField] private RawImage _thumbnail;
        [SerializeField] private bool _adjustAspectRatio;
        [SerializeField][ShowIf(nameof(_adjustAspectRatio))] private AspectRatioFitter _aspectRatioFitter;

        protected override InitializationResult OnInitialize(BaseProfilePhotoPreviewPageArgs args, CancellationToken token)
        {
            var thumbnail = args.Photo;
            _thumbnail.texture = thumbnail;
            if (_adjustAspectRatio)
            {
                _aspectRatioFitter.aspectRatio = (float)thumbnail.width / thumbnail.height;
            }

            return InitializationResult.Done;
        }

        protected override void OnInitializationCancelled() { }

        protected override void OnShowContent()
        {
            _thumbnail.color = Color.white;
            _thumbnail.enabled = true;
        }

        protected override void OnCleanUp()
        {
            if (_thumbnail.texture) return;
            
            Destroy(_thumbnail.texture);
            _thumbnail.texture = null;
        }
    }
}
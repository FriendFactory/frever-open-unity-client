using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class AssetSnackBar: SnackBar<AssetSnackBarConfiguration>
    {
        [SerializeField] private Image _thumbnail;
        [SerializeField] private AspectRatioFitter _thumbnailFitter;
        
        public override SnackBarType Type => SnackBarType.AssetClaimed;
        
        protected override void OnConfigure(AssetSnackBarConfiguration configuration)
        {
            if (!configuration.Thumbnail)
            {
                return;
            }

            var thumbnail = configuration.Thumbnail;
            _thumbnail.sprite = thumbnail;
            _thumbnail.preserveAspect = true;
            _thumbnailFitter.aspectRatio = thumbnail.texture.width / (float)thumbnail.texture.height;
        }

        public override void OnHidden()
        {
            base.OnHidden();
            
            Destroy(_thumbnail.sprite);
        }
    }
}
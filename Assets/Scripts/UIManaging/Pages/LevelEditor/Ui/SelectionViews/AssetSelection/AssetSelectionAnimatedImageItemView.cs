using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public class AssetSelectionAnimatedImageItemView : AssetSelectionAnimatedItemView
    {
        private const long EMPTY_ITEM_ID = -1;
        
        private Vector2 _originalAnchorMin;
        private Vector2 _originalAnchorMax;
    
        protected override string TitleDisplayText => ContextData.DisplayName;

        protected override void Awake()
        {
            base.Awake();
            
            if (_rawImage)
            {
                _originalAnchorMin = _rawImage.rectTransform.anchorMin;
                _originalAnchorMax = _rawImage.rectTransform.anchorMax;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        
            if (!IsSameItem())
            {
                LastItemId = ContextData.ItemId;
                LastItemType = ContextData.GetType();
                
                CleanupRawImage();
                DownloadThumbnail();
            }
        }

        protected override void OnThumbnailLoaded(long id, object downloadedTexture)
        {
            var texture = (Texture2D)downloadedTexture;
            if (texture == null) return;

            if (IsDestroyed || !gameObject.activeInHierarchy || ContextData.ThumbnailOwner.Id != id)
            {
                Destroy(texture);
                return;
            }

            var aspectRatio = texture.width / (float)texture.height;
            CorrectImageSizeToAspectRatio(aspectRatio);
            _rawImage.texture = texture;
        
            ShowRawImage();
        }

        private void CorrectImageSizeToAspectRatio(float ratio)
        {
            var rectTransform = _rawImage.rectTransform;
            var newAnchorMin = new Vector2(_originalAnchorMin.x, _originalAnchorMin.y);
            var newAnchorMax = new Vector2(_originalAnchorMax.x, _originalAnchorMax.y);
            if (ratio < 1)
            {
                var offsetX = (1 - ratio) / 2;
                newAnchorMin.x += offsetX;
                newAnchorMax.x -= offsetX;            
            }
            else
            {
                var offsetY = (1 - 1 / ratio) / 2;
                newAnchorMin.y += offsetY;
                newAnchorMax.y -= offsetY;
            }
            rectTransform.anchorMin = newAnchorMin;
            rectTransform.anchorMax = newAnchorMax;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            LastItemId = EMPTY_ITEM_ID;
            LastItemType = null;
        }
    }
}

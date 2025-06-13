using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal class SubAssetSelectionImageItemView : AssetSelectionItemView
    {
        private const float SCALE_DURATION = 0.2f;
        
        [SerializeField] private Image _marker;
        private readonly Vector3 _scaleSize = new Vector3(1.2f, 1.2f, 0f);

        private Tween _scaleTween;
        private Tween _showMarkerTween;
        private RectTransform _rect;

        protected override void OnInitialized()
        {
            _rect = GetComponent<RectTransform>();
            base.OnInitialized();
        
            if (!IsSameItem())
            {
                CleanupRawImage();
                DownloadThumbnail();
            }
        }

        protected override void OnThumbnailLoaded(long id, object downloadedTexture)
        {
            if (IsDestroyed || !gameObject.activeInHierarchy || ContextData.ThumbnailOwner.Id != id) return;
        
            _rawImage.texture = (Texture2D) downloadedTexture;
            ShowRawImage();
        }

        protected override void RefreshSelectionGameObjects()
        {
            base.RefreshSelectionGameObjects();
            ScaleItem(ContextData.IsSelected);
            ShowMarker(ContextData.IsSelected);
        }

        private void ScaleItem(bool isSelected)
        {
            if (IsDestroyed) return;

            _scaleTween.Kill();
            _scaleTween = _rect.DOScale(isSelected ? _scaleSize : Vector3.one, SCALE_DURATION).SetUpdate(true);
        }

        private void ShowMarker(bool isSelected)
        {
            if (IsDestroyed) return;

            _showMarkerTween?.Kill();
            _showMarkerTween = _marker.DOFade(isSelected ? 1f : 0f, SCALE_DURATION).SetUpdate(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (IsDestroyed) return;
            _scaleTween.Kill(true);
            _showMarkerTween.Kill(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _scaleTween.Kill();
            _showMarkerTween.Kill();
        }
    }
}

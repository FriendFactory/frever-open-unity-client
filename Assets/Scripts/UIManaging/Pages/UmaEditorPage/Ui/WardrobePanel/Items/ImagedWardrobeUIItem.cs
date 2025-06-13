using System;
using Bridge.Models.Common;
using UnityEngine;
using UnityEngine.UI;
using Resolution = Bridge.Models.Common.Files.Resolution;
using System.Threading;
using Extensions;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class ImagedWardrobeUIItem<T> : EntityUIItem<T> where T: IThumbnailOwner
    {
        [SerializeField]
        protected Image _thumbnail;
        [SerializeField]
        protected GameObject _loadingCircle;
        [SerializeField]
        protected GameObject _placeholderImage;

        [NonSerialized]
        public Resolution ThumbnailResolution = Resolution._128x128;

        private Sprite _thumbnailSprite;
        private CancellationTokenSource _cancellationTokenSource;

        protected virtual void OnDestroy()
        {
            if (_thumbnailSprite != null)
            {
                Destroy(_thumbnailSprite.texture);
                Destroy(_thumbnailSprite);
            }

            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
        }

        public override void Setup(T entity)
        {
            base.Setup(entity);
            
            if (_thumbnailSprite != null)
            {
                Destroy(_thumbnailSprite.texture);
                Destroy(_thumbnailSprite);
            }
            if (_placeholderImage != null)
            {
                _placeholderImage.SetActive(true);
            }
            LoadThumbnail();
        }

        public void SetActiveLoading(bool active)
        {
            _loadingCircle.gameObject.SetActive(active);
        }

        protected override void ChangeSelectionVisual()
        {
        }

        protected virtual async void LoadThumbnail()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = new CancellationTokenSource();

            var result = await _bridge.GetThumbnailAsync(Entity, ThumbnailResolution, cancellationToken: _cancellationTokenSource.Token);

            if (result.IsRequestCanceled) return;

            if (result.IsSuccess)
            {
                OnThumbnailLoaded(result.Object);
            }
            else
            {
                Debug.LogWarning(result.ErrorMessage);
            }
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void OnThumbnailLoaded(object obj)
        {
            if (obj is Texture2D thumbnailTexture && !_thumbnail.IsDestroyed())
            {
                var rect = new Rect(0.0f, 0.0f, thumbnailTexture.width, thumbnailTexture.height);
                var pivot = new Vector2(0.5f, 0.5f);
                _thumbnailSprite = Sprite.Create(thumbnailTexture, rect, pivot);
                _thumbnail.sprite = _thumbnailSprite;
                if (_placeholderImage != null)
                {
                    _placeholderImage.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Wrong thumbnail format");
            }
        }
    }
}

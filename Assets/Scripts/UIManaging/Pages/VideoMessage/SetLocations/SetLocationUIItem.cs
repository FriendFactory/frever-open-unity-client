using System;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using EnhancedUI.EnhancedScroller;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.VideoMessage.SetLocations
{
    internal sealed class SetLocationUIItem : EnhancedScrollerCellView
    {
        [SerializeField] private Resolution _thumbnailResolution = Resolution._256x256;
        [SerializeField] private GameObject[] _selectionGameObjects;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;

        [Inject] private IBridge _bridge;
        
        private CancellationTokenSource _cancellationTokenSource;
        private Button _button;
        private Sprite _thumbnailSprite;
        private SetLocationUIItemModel _model;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(SetLocationUIItemModel model)
        {
            _model = model;
            RefreshSelectionState();
            DestroyPreviousImage();
            SetAlpha(0);
            _text.text = model.SetLocationFullInfo.Name;
            LoadThumbnail();
        }
        
        public void RefreshSelectionState()
        {
            foreach (var selectionGameObject in _selectionGameObjects)
            {
                selectionGameObject.SetActive(_model.IsSelected);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClick()
        {
            _model.OnClick(_model.SetLocationFullInfo);
        }

        private async void LoadThumbnail()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = new CancellationTokenSource();

            var result = await _bridge.GetThumbnailAsync(_model.SetLocationFullInfo, _thumbnailResolution, cancellationToken: _cancellationTokenSource.Token);

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
            if (obj is Texture2D thumbnailTexture && !_image.IsDestroyed())
            {
                DestroyPreviousImage();
                SetAlpha(1);
                ApplyImage(thumbnailTexture);
            }
            else
            {
                Debug.LogWarning("Wrong thumbnail format");
            }
        }

        private void ApplyImage(Texture2D thumbnailTexture)
        {
            var rect = new Rect(0.0f, 0.0f, thumbnailTexture.width, thumbnailTexture.height);
            var pivot = new Vector2(0.5f, 0.5f);
            _thumbnailSprite = Sprite.Create(thumbnailTexture, rect, pivot);
            _image.sprite = _thumbnailSprite;
        }

        private void DestroyPreviousImage()
        {
            if (_image.sprite == null) return;
            Destroy(_image.sprite.texture);
            Destroy(_image.sprite);
            _image.sprite = null;
        }

        private void SetAlpha(float alpha)
        {
            _image.SetAlpha(alpha);
        }
    }

    internal sealed class SetLocationUIItemModel
    {
        public SetLocationFullInfo SetLocationFullInfo;
        public bool IsSelected;
        public Action<SetLocationFullInfo> OnClick;
    }
}

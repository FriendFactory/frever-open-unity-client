using System.Threading;
using Abstract;
using Bridge.Models.Common;
using DG.Tweening;
using Extensions;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public sealed class EventThumbnail : BaseContextDataView<IEventInfo>
    {
        [SerializeField] private Resolution _resolution = Resolution._128x128;
        [SerializeField] private RawImage _thumbnailRawImage;
        [SerializeField] private GameObject _loadingIconGameObject;
       
        [Inject] private VideoManager _videoManager;

        private CancellationTokenSource _tokenSource;

        private void Awake()
        {
            if (_loadingIconGameObject != null)
            {
                _loadingIconGameObject.SetActive(false);
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError("Missing reference to loading icon game object." +
                               " Can't happen under normal circumstances. Please reimport your project.");
            }
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DisposeTokenSource();
        }

        protected override void OnInitialized()
        {
            RefreshThumbnail();
        }

        public void RefreshThumbnail()
        {
            CancelThumbnailLoading();

            if (ContextData == null)
            {
                _loadingIconGameObject.SetActive(true);
                _thumbnailRawImage.color = Color.white.SetAlpha(0f);
                return;
            }
            _tokenSource = new CancellationTokenSource();
            _videoManager.GetThumbnailForEvent(ContextData, _resolution, OnThumbnailDownloaded, _tokenSource.Token);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _thumbnailRawImage.texture = null;
            _thumbnailRawImage.gameObject.SetActive(false);
        }

        private void OnThumbnailDownloaded(Texture2D thumbnail)
        {
            if (IsDestroyed) return;
            _thumbnailRawImage.texture = thumbnail;
            _thumbnailRawImage.DOColor(Color.white, 0.3f).SetUpdate(true);
            _thumbnailRawImage.gameObject.SetActive(true);
            _loadingIconGameObject.SetActive(false);
        }
        
        private void CancelThumbnailLoading()
        {
            _tokenSource?.Cancel();
            DisposeTokenSource();
        }

        private void DisposeTokenSource()
        {
            _tokenSource?.Dispose();
            _tokenSource = null;
        }
    }
}

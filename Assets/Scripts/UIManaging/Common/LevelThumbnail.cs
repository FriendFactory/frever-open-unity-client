using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Abstract;
using Resolution = Bridge.Models.Common.Files.Resolution;
using System.Threading;
using Models;

namespace UIManaging.Common
{
    public class LevelThumbnail : BaseContextDataView<Level>
    {
        [SerializeField] private RawImage _levelThumbnail;
        [SerializeField] private Resolution _thumbnailResolution = Resolution._512x512;
        [SerializeField] private Texture2D _defaultThumbnail;

        [Inject] private VideoManager _videoManager;

        private CancellationTokenSource _cancellationSource;
        private bool _isDefaultThumbnail;
        private Texture2D _loadedTexture;

        public void DisplayNonLevelVideoThumbnail()
        {
            SetThumbnail(_defaultThumbnail);
            _isDefaultThumbnail = true;
        }
        
        protected override void OnInitialized()
        {
            CancelThumbnailLoading();
            _cancellationSource = new CancellationTokenSource();

            if (ContextData != null)
            {
                _videoManager.GetThumbnailForLevel(ContextData, _thumbnailResolution, SetThumbnail, _cancellationSource.Token);
                _isDefaultThumbnail = false;
            }
            else
            {
                DisplayNonLevelVideoThumbnail();
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            CancelThumbnailLoading();
            if(!_isDefaultThumbnail && _levelThumbnail.texture) Destroy(_loadedTexture);
            _loadedTexture = null;
            _levelThumbnail.texture = null;
            _levelThumbnail.gameObject.SetActive(false);
        }

        private void SetThumbnail(Texture2D thumbnail)
        {
            if (IsDestroyed || _levelThumbnail.IsDestroyed())
            {
                Destroy(thumbnail);
                return;
            }

            _loadedTexture = thumbnail;
            _levelThumbnail.texture = thumbnail;
            _levelThumbnail.gameObject.SetActive(true);
        }

        private void CancelThumbnailLoading()
        {
            _cancellationSource?.Cancel();
            _cancellationSource = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanUp();
        }
    }
}
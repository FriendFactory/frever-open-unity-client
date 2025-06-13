using System;
using System.Linq;
using System.Threading;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.AssetStore;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;
using Bridge.Results;
using Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common
{
    public class ThumbnailLoader : BaseContextDataView<IThumbnailOwner>
    {
        [SerializeField] private RawImage _thumbnail;
        [SerializeField] private AspectRatioFitter _thumbnailFitter;
        [SerializeField] private FileType _fileType;
        [SerializeField] private Resolution _resolution = Resolution._256x256;
        [SerializeField] private ZenProjectContextInjecter _injecter;
        private IBridge _bridge;

        private Texture _loadedTexture;
        private CancellationTokenSource _cancellationSource;

        public event Action OnThumbnailReady;
        
        [Inject]
        [UsedImplicitly]
        public void Construct(IBridge bridge)
        {
            _bridge = bridge;
        }

        protected override void OnInitialized()
        {
            if (_injecter && _injecter.enabled)
            {
                _injecter.Awake();
                _injecter.enabled = false;
            }
            
            _thumbnail.enabled = false;
            _cancellationSource = new CancellationTokenSource();

            if (ContextData.Files.IsNullOrEmpty())
            {
                return;
            }

            LoadThumbnail(_fileType, _resolution);
        }

        private async void LoadThumbnail(FileType fileType, Resolution resolution)
        {
            if (ContextData.Files.All(file => file.Resolution != _resolution))
            {
                var firstFile = ContextData.Files.FirstOrDefault();

                if (firstFile != null)
                {
                    fileType = firstFile.FileType;
                    resolution = firstFile.Resolution ?? Resolution._256x256;
                }
            }

            GetAssetResult result;

            if (ContextData is AssetInfo assetInfo)
            {
                result = await _bridge.GetThumbnailAsync(assetInfo, resolution, true, _cancellationSource.Token);
            }
            else if (fileType == FileType.MainFile)
            {
                result = await _bridge.GetAssetAsync(ContextData, true, _cancellationSource.Token);
            }
            else
            {
                result = await _bridge.GetThumbnailAsync(ContextData, resolution, true, _cancellationSource.Token);
            }
            
            _cancellationSource?.Dispose();
            _cancellationSource = null;

            if (!result.IsSuccess) return;
            
            if (result.Object is Texture2D texture)
            {
                OnThumbnailLoaded(texture);
            }
        }

        public void CancelThumbnailLoading()
        {
            _cancellationSource?.CancelAndDispose();
            _cancellationSource = null;
        }
        
        private void OnThumbnailLoaded(Texture texture)
        {
            if (_thumbnail.IsDestroyed()) return;
            _thumbnail.enabled = true;
            _loadedTexture = texture;
            _thumbnail.texture = texture;
            _thumbnailFitter.aspectRatio = (float) texture.width / texture.height;
            
            OnThumbnailReady?.Invoke();
        }

        protected override void BeforeCleanup()
        {
            CancelThumbnailLoading();

            if (!_loadedTexture) return;

            DestroyImmediate(_loadedTexture, true);
            _loadedTexture = null;
        }
    }
}
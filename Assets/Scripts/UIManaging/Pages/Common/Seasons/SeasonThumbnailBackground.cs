using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Common.UI;
using Modules.AssetsStoraging.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Common.Seasons
{
    public class SeasonThumbnailBackground: UIElementWithPlaceholder<Resolution>
    {
        [SerializeField] private RawImage _backgroundImage;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;

        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private IBridge _bridge;
        
        private Texture _backgroundTexture;
        private Color _placeholderColor;
        private int _index = 0;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _placeholderColor = _backgroundImage.color;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override Task InitializeAsync(Resolution model, CancellationToken token)
        {
            if (IsInitialized || _dataFetcher.CurrentSeason == null)
            {
                return Task.CompletedTask;
            }

            return base.InitializeAsync(model, token);
        }

        /// <summary>
        /// Sets index of the marketing thumbnail, by default index is set to 0
        /// </summary>
        /// <param name="index"></param>
        public void SetImageIndex(int index)
        {
            _index = index;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override InitializationResult OnInitialize(Resolution resolution, CancellationToken token)
        {
            _backgroundImage.enabled = false;
            DownloadBackgroundTexture(resolution, token);

            return InitializationResult.Wait;
        }

        protected override void OnInitializationCancelled() { }

        protected override void OnShowContent()
        {
            if (!_backgroundTexture)
            {
                return;
            }
            
            _aspectRatioFitter.aspectRatio = (float)_backgroundTexture.width / _backgroundTexture.height;

            _backgroundTexture.wrapMode = TextureWrapMode.Clamp;
            _backgroundImage.color = Color.white;
            _backgroundImage.texture = _backgroundTexture;
            _backgroundImage.enabled = true;
        }

        protected override void OnCleanUp()
        {
            if (!_backgroundTexture)
            {
                return;
            }
            
            _backgroundImage.color = _placeholderColor;
            _backgroundImage.texture = null;

            Destroy(_backgroundTexture); 
            _backgroundTexture = null;
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void OnTextureDownloaded(Texture texture2D)
        {
            _backgroundTexture = texture2D;
            CompleteInitialization();
        }

        private void OnTextureFailedToDownload(string reason)
        {
            CompleteInitialization();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void DownloadBackgroundTexture(Resolution resolution, CancellationToken token)
        {
            var marketingScreenshots = _dataFetcher.CurrentSeason.MarketingScreenshots;

            if ((marketingScreenshots?.Count ?? 0) <= _index)
            {
                return;
            }

            var result = await _bridge.GetThumbnailAsync(marketingScreenshots[_index], resolution, cancellationToken: token);

            if(token.IsCancellationRequested) return;
            
            if (result.IsError)
            {
                OnTextureFailedToDownload(result.ErrorMessage);
                return;
            }

            if (result.IsSuccess)
            {
                OnTextureDownloaded(result.Object as Texture2D);
            }
        }
    }
}
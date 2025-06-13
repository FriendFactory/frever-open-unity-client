using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Modules.AssetsStoraging.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;
using Texture = UnityEngine.Texture;

namespace UIManaging.Pages.Tasks
{
    internal sealed class TaskHeaderBackground : MonoBehaviour
    {
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private IBridge _bridge;
        
        [SerializeField] private GameObject _activeBackground;
        [SerializeField] private GameObject _inactiveBackground;

        [Space]
        [SerializeField] private GameObject _seasonBackgroundParent;
        [SerializeField] private RawImage _seasonBackground;

        private static Texture2D _seasonTexture;
        private bool _useSeasonBackground;

        public void OnEnable()
        {
            if (_useSeasonBackground && !_seasonBackgroundParent.activeSelf)
            {
                SetupSeasonBackground();
            }
        }

        public void Setup(TaskType taskType, bool isActive)
        {
            _activeBackground.SetActive(false);
            _inactiveBackground.SetActive(false);
            _seasonBackgroundParent.SetActive(false);
            
            SetUpNormalBackground(isActive);

            _useSeasonBackground = taskType == TaskType.Season && isActive;
            if (!gameObject.activeSelf || !gameObject.activeInHierarchy)
            {
                return;
            }

            if (_useSeasonBackground)
            {
                SetupSeasonBackground();
            }
        }

        private async void SetupSeasonBackground()
        {
            if (_seasonTexture == null)
            {
                _seasonTexture = await GetSeasonTexture();
            }

            _seasonBackgroundParent.SetActive(true);
            _seasonBackground.texture = _seasonTexture;
        }

        private async Task<Texture2D> GetSeasonTexture()
        {
            var marketingScreenshot =_dataFetcher.CurrentSeason.MarketingScreenshots[0];
            var result = await _bridge.GetThumbnailAsync(marketingScreenshot, Resolution._512x512);

            if (result.IsError)
            {
                Debug.LogError(result);
                return null;
            }

            return result.Object as Texture2D;
        }
        
        private void SetUpNormalBackground(bool isActive)
        {
            _activeBackground.SetActive(isActive);
            _inactiveBackground.SetActive(!isActive);
        }
    }
}
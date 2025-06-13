using System.Threading;
using Bridge;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal sealed class ImageThumbnailAssetItemView : RewardItemView
    {
        [SerializeField] private Image _image;
        [Inject] private IBridge _bridge;
        
        private CancellationTokenSource _cancellationTokenSource;

        public override void Setup(SeasonRewardItemModel model)
        {
            LoadIconAsync(model);
            Text.text = "x1";
        }
        private async void LoadIconAsync(SeasonRewardItemModel model)
        {
            _image.SetAlpha(0);
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var thumbnailResp = await _bridge.GetThumbnailAsync(model.Asset, Resolution._128x128, true, _cancellationTokenSource.Token);
            if (thumbnailResp.IsSuccess)
            {
                var icon = (thumbnailResp.Object as Texture2D).ToSprite();
                _image.sprite = icon;
                _image.SetAlpha(1);
            }
            else
            {
                Debug.LogWarning($"Failed to load icon for reward. {model.Asset} {model.Asset.Id}");
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.CancelAndDispose();
        }
    }
}
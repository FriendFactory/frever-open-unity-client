using System.Threading;
using Bridge;
using Extensions;
using OldMoatGames;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal sealed class GifThumbnailAssetItemView : RewardItemView
    {
        [SerializeField] private AnimatedGifPlayer _gifPlayer;
        [Inject] private IBridge _bridge;
        
        private CancellationTokenSource _cancellationTokenSource;

        public override void Setup(SeasonRewardItemModel model)
        {
            LoadIconAsync(model);
            Text.text = "x1";
        }
        private async void LoadIconAsync(SeasonRewardItemModel model)
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var thumbnailResp = await _bridge.GetThumbnailAsync(model.Asset, Resolution._128x128, true, _cancellationTokenSource.Token);
            if (thumbnailResp.IsSuccess)
            {
                _gifPlayer.AutoPlay = true;
                _gifPlayer.Path = GifPath.PersistentDataPath;
                _gifPlayer.GifBytes = (byte[])thumbnailResp.Object;
                _gifPlayer.Init();
                _gifPlayer.Pause();
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
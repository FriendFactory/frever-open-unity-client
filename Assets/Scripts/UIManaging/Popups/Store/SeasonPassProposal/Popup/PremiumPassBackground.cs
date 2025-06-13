using System.Threading;
using Bridge;
using Extensions;
using Modules.InAppPurchasing;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Popups.Store.SeasonPassProposal.Popup
{
    internal sealed class PremiumPassBackground: MonoBehaviour
    {
        [SerializeField] private Image _image;
        [Inject] private IBridge _bridge;
        [Inject] private IIAPManager _iapManager;
        private bool _hasThumbnail;

        private CancellationTokenSource _cancellationTokenSource;

        public async void Setup()
        {
            if (_hasThumbnail) return;
            
            var offer = _iapManager.GetSeasonPassProduct().ProductOffer;
            if (offer.Files == null) return;
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            var resp = await _bridge.GetThumbnailAsync(offer, Resolution._1024x1024, true, _cancellationTokenSource.Token);
            if (resp.IsSuccess)
            {
                var texture2d = resp.Object as Texture2D;
                _image.sprite = texture2d.ToSprite();
                _image.preserveAspect = true;
                _image.enabled = true;
                _hasThumbnail = true;
            }
        }

        private void OnDisable()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
        }
    }
}
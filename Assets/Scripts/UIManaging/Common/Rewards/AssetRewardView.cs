using System.Threading.Tasks;
using Bridge.Models.ClientServer.Gamification.Reward;
using Bridge.Results;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common.Rewards
{
    public class AssetRewardView : RewardView
    {
        [SerializeField] private AspectRatioFitter _thumbnailFitter;
        
        private float _defaultThumbnailRatio = 1;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _defaultThumbnailRatio = _thumbnailFitter.aspectRatio;
            _thumbnailFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        }
        
        //---------------------------------------------------------------------
        // IRewardView
        //---------------------------------------------------------------------

        public override void Show(IRewardModel reward, RewardState state)
        {
            base.Show(reward, state);
            _thumbnailFitter.aspectRatio = _defaultThumbnailRatio;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected override void ShowAsAvailable()
        {
            base.ShowAsAvailable();
            
            _thumbnailBackground.sprite = _defaultBackground;
            _thumbnail.SetActive(false);
            DownloadThumbnail();
        }

        protected override void ShowAsClaimed(bool updateThumbnail = true)
        {
            base.ShowAsClaimed(updateThumbnail);

            if (!updateThumbnail) return;

            DownloadThumbnail();
        }

        protected override void ShowAsObtainable()
        {
            base.ShowAsObtainable();
            
            _thumbnailBackground.sprite = _defaultBackground;
            DownloadThumbnail();
        }

        protected override void ShowAsLocked()
        {
            base.ShowAsLocked();

            _thumbnailBackground.sprite = _defaultBackground;
            DownloadThumbnail();
        }

        protected override void OnClaimButtonClicked()
        {
            _button.onClick.RemoveAllListeners();
            PageModel.OnAssetRewardClaimed(Reward.Id, _thumbnail.sprite, OnClaimResult);
        }

        protected override Task<GetAssetResult> GetThumbnailRequest()
        {
            return Bridge.GetThumbnailAsync(Reward.Asset, Resolution._256x256, true, CancellationSource.Token);
        }

        protected override void OnThumbnailLoaded(Texture2D texture)
        {
            base.OnThumbnailLoaded(texture);
            
            _thumbnailFitter.aspectRatio = (float) texture.width / texture.height;
        }
    }
}
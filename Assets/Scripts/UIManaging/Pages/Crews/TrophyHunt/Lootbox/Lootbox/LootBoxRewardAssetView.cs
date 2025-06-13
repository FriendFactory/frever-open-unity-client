using Abstract;
using Bridge.Models.Common;
using Common.UI;
using DG.Tweening;
using Extensions;
using UIManaging.Pages.Crews.TrophyHunt.Lootbox.Lootbox;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews.TrophyHunt.Lootbox
{
    public class LootBoxRewardAssetView : BaseContextDataView<LootboxRewardModel>
    {
        [SerializeField]
        private PriceHolder _priceHolder;
        [SerializeField]
        private TierBackgroundUI _tierBackgroundUI;
        [SerializeField]
        private Image _selectionGameObject;
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private RawImage _thumbnail;

        public void SetAsSelected()
        {
            _selectionGameObject.DOFade(1, 0.1f);
            _priceHolder.SetCost(null, true);
        }
        
        private void SetupPrice()
        {
            _priceHolder.SetActive(true);
            var offer = new AssetOfferInfo()
            {
                AssetId = ContextData.Asset.Id,
                AssetOfferSoftCurrencyPrice = ContextData.Asset.SoftCurrency,
                AssetOfferHardCurrencyPrice = ContextData.Asset.HardCurrency
            };
            _priceHolder.SetCost(offer);
        }

        private void SetupTier()
        {
            _tierBackgroundUI.SetActive(true);
            _tierBackgroundUI.SetTier(ContextData.Asset.AssetTierId);
        }

        public void FadeOut()
        {
            _canvasGroup.DOFade(0f, 0.25f);
        }

        protected override void OnInitialized()
        {
            SetupTier();
            SetupPrice();
            _canvasGroup.alpha = 1;
            _selectionGameObject.color = Color.clear;
            _thumbnail.texture = ContextData.Thumbnail;
        }
    }
}
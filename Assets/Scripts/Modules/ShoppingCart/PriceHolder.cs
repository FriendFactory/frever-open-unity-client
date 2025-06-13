using UnityEngine;
using TMPro;
using Bridge.Models.Common;
using UnityEngine.Serialization;
using Zenject;
using UIManaging.Localization;

namespace Common.UI
{
    public class PriceHolder : MonoBehaviour
    {
        private static readonly int APPEAR_HASH = Animator.StringToHash("Appear");
        
        [SerializeField]
        private TextMeshProUGUI _priceText;
        [SerializeField]
        private int _softCurrencyIconIndex;
        [SerializeField]
        private int _hardCurrencyIconIndex;
        [SerializeField]
        private GameObject _ownedIcon;
        [SerializeField]
        private Animator _ownedAnimator;
        [SerializeField]
        private int _currencyIconScale = 110;

        [Inject] private ShoppingCartLocalization _localization;

        public void SetCost(AssetOfferInfo assetOffer, bool isOwned = false)
        {
            var cost = 0;
            var isHard = false;
            if (assetOffer != null)
            {
                if (assetOffer.AssetOfferHardCurrencyPrice.HasValue)
                {
                    cost = assetOffer.AssetOfferHardCurrencyPrice.Value;
                    isHard = true;
                }
                else if (assetOffer.AssetOfferSoftCurrencyPrice.HasValue)
                {
                    cost = assetOffer.AssetOfferSoftCurrencyPrice.Value;
                    isHard = false;
                }
            }
            SetCost(cost, isHard, isOwned);
        }

        public void SetCost(int cost, bool inHardCurrency, bool isOwned = false)
        {
            if (_ownedIcon != null) _ownedIcon.SetActive(isOwned);
            _priceText.gameObject.SetActive(!isOwned);

            if (isOwned)
            {
                return;
            }

            if (cost == 0)
            {
                _priceText.text = _localization.FreePriceText;
                return;
            }

            _priceText.text = 
                $"<size={_currencyIconScale}%><sprite index={(inHardCurrency ? _hardCurrencyIconIndex : _softCurrencyIconIndex)}></size> {cost}";
        }

        public void AnimateOwnedIcon()
        {
            _ownedAnimator.SetTrigger(APPEAR_HASH);
        }
    }
}

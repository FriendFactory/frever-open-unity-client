using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonPremiumPanel : MonoBehaviour
    {
        [SerializeField] private Button _buyPremiumButton;
        [SerializeField] private GameObject _hardCurrencyPrice;
        [SerializeField] private GameObject _takeForFreeText;
        [SerializeField] private TMP_Text _premiumPriceText;
        [SerializeField] private GameObject _unlockedPremiumBadge;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            _buyPremiumButton.onClick.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UpdatePremiumState(bool isPremium, int premiumPrice, UnityAction premiumButtonCallback)
        {
            if (!isPremium)
            {
                var isFree = premiumPrice == 0;
                _hardCurrencyPrice.SetActive(!isFree);
                _takeForFreeText.SetActive(isFree);
                if (!isFree)
                {
                    _premiumPriceText.text = premiumPrice.ToString();
                }
                _buyPremiumButton.gameObject.SetActive(true);
                _buyPremiumButton.onClick.RemoveAllListeners();
                _buyPremiumButton.onClick.AddListener(premiumButtonCallback);
            }
            else
            {
                _buyPremiumButton.gameObject.SetActive(false);
                _unlockedPremiumBadge.SetActive(true);
            }
        }
    }
}
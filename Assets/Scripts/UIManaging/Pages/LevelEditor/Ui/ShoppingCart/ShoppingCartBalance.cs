using Modules.Amplitude;
using JetBrains.Annotations;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public class ShoppingCartBalance : MonoBehaviour
    {
        [SerializeField] private ShoppingCartManager _cartManager;
        [Space]
        [SerializeField] private TextMeshProUGUI _softCurrencyAmount;
        [SerializeField] private GameObject _softCurrencyRedFlag;
        [Space]
        [SerializeField] private TextMeshProUGUI _hardCurrencyAmount;
        [SerializeField] private GameObject _hardCurrencyRedFlag;

        private LocalUserDataHolder _userData;
        private AmplitudeManager _amplitudeManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(LocalUserDataHolder userDataHolder, AmplitudeManager amplitudeManager)
        {
            _userData = userDataHolder;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            if (_amplitudeManager.IsShoppingCartFeatureEnabled())
            {
                UpdateCurrencyAmounts();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateCurrencyAmounts()
        {
            var softCurrencyAmount = _userData.UserBalance.SoftCurrencyAmount;
            var softPrice = _cartManager.TotalSoftPrice + _cartManager.GetSoftPriceForSelectedAssets();
            _softCurrencyAmount.text = softCurrencyAmount.ToString();
            _softCurrencyRedFlag.SetActive(softCurrencyAmount < softPrice);

            var hardCurrencyAmount = _userData.UserBalance.HardCurrencyAmount;
            var hardPrice = _cartManager.TotalHardPrice + _cartManager.GetHardPriceForSelectedAssets();
            _hardCurrencyAmount.text = hardCurrencyAmount.ToString();
            _hardCurrencyRedFlag.SetActive(hardCurrencyAmount < hardPrice);
        }
    }
}
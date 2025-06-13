using Modules.InAppPurchasing;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using Zenject;

namespace UIManaging.Popups.Store
{
    internal sealed class BuyCoinsItemView : BuyCurrencyItemView<SoftCurrencyProduct>
    {
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private LocalUserDataHolder _localUserDataHolder ;
        
        protected override void OnBuyClicked()
        {
            base.OnBuyClicked();
            var userHardCurrency = _localUserDataHolder.UserBalance.HardCurrencyAmount;
            var hasEnoughFunds = userHardCurrency >= int.Parse(ContextData.PriceText);

            if (hasEnoughFunds)
            {
                _popupManagerHelper.ShowConfirmPopup(
                    ()=>IAPManager.BuySoftCurrency(ContextData.ReferenceId),
                    ContextData.Amount, 
                    ContextData.PriceText);
            }
            else
            {
                _popupManagerHelper.ShowNotEnoughFundsPopup(false, OnNotEnoughCurrencyForPurchase);
            }

            ShowLoadingUI(false);
        }
    }
}

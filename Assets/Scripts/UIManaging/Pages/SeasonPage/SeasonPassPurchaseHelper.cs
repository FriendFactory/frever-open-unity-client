using System;
using Modules.InAppPurchasing;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonPassPurchaseHelper: MonoBehaviour
    {
        [Inject] private IIAPManager _iapManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private SnackBarHelper _snackbarHelper;
        [Inject] private PurchasesLocalization _purchasesLocalization;

        public event Action PremiumPassPurchased;

        private void OnEnable()
        {
            _iapManager.PurchasedSeasonPass += OnSeasonPassPurchased;
            _iapManager.PurchaseFailed += OnPurchaseFailed;
        }

        private void OnDisable()
        {
            _iapManager.PurchasedSeasonPass -= OnSeasonPassPurchased;
            _iapManager.PurchaseFailed -= OnPurchaseFailed;
        }

        private void OnSeasonPassPurchased(SeasonPassProduct seasonPass)
        {
            if (!seasonPass.IsFree)
            {
                _snackbarHelper.ShowPurchaseSuccessSnackBar(_purchasesLocalization.PurchaseCompletedMessage);
            }

            _popupManagerHelper.HidePremiumPassPopup();
            _popupManagerHelper.ShowPremiumPassPurchaseSucceedPopup(true, HidePremiumPassPurchaseSucceedPopup, HidePremiumPassPurchaseSucceedPopup);
            PremiumPassPurchased?.Invoke();
        }

        private void HidePremiumPassPurchaseSucceedPopup()
        {
            _popupManagerHelper.HidePremiumPassPurchaseSucceedPopup();
        }

        private void OnPurchaseFailed()
        {
            _snackbarHelper.ShowPurchaseFailedSnackBar(_purchasesLocalization.PurchaseFailedMessage);
        }
    }
}
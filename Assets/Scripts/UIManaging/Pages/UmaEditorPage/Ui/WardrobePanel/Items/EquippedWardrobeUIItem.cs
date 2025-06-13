using System;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using UnityEngine;
using Common.UI;
using Extensions;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class EquippedWardrobeUIItem : WardrobeUIItem
    {
        protected override void SetupTier()
        {
            base.SetupTier();
            if (!_hasTier)
            {
                return;
            }

            switch (Entity)
            {
                case WardrobeShortInfo wardrobe:
                    _hasTier = !IsFree(wardrobe.AssetOffer);
                    break;
                case WardrobeFullInfo wardrobe:
                    _hasTier = !IsFree(wardrobe.AssetOffer);
                    break;
            }
            
            _tierBackgroundUI.SetActive(_hasTier);

        }

        private bool IsFree(AssetOfferInfo offerInfo)
        {
            if (offerInfo is null)
            {
                return true;
            }
            if (offerInfo.AssetOfferHardCurrencyPrice.HasValue)
            {
                return offerInfo.AssetOfferHardCurrencyPrice.Value == 0;
            } 
            else if (offerInfo.AssetOfferSoftCurrencyPrice.HasValue)
            {
                return offerInfo.AssetOfferSoftCurrencyPrice == 0;
            }

            return true;
        }
    }
}

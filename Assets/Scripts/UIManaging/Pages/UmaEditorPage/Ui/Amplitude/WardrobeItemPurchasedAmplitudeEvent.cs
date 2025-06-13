using Bridge.Models.ClientServer.Assets;
using Modules.Amplitude.Events.Core;

namespace UIManaging.Pages.UmaEditorPage.Ui.Amplitude
{
    internal sealed class WardrobeItemPurchasedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => "purchase_wardrobe_item";

        public WardrobeItemPurchasedAmplitudeEvent(WardrobeShortInfo wardrobeItem)
        {
            _eventProperties.Add("Asset ID", wardrobeItem.Id);
            
            var softCurrencyPrice = wardrobeItem.AssetOffer.AssetOfferSoftCurrencyPrice;
            var hardCurrencyPrice = wardrobeItem.AssetOffer.AssetOfferHardCurrencyPrice;

            _eventProperties.Add("Soft Currency Price", softCurrencyPrice.HasValue ? softCurrencyPrice : "null");
            _eventProperties.Add("Hard Currency Price", hardCurrencyPrice.HasValue ? hardCurrencyPrice : "null");
        }
    }
}
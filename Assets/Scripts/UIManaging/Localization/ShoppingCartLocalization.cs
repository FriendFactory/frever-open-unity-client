using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/ShoppingCartLocalization", fileName = "ShoppingCartLocalization")]
    public class ShoppingCartLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _buyButtonBuy;
        [SerializeField] private LocalizedString _buyButtonLocked;
        [SerializeField] private LocalizedString _buyButtonDone;
        [SerializeField] private LocalizedString _itemsCounterTextFormat;
        [SerializeField] private LocalizedString _freePriceText;
        
        public string BuyButtonBuy => _buyButtonBuy;
        public string BuyButtonLocked => _buyButtonLocked;
        public string BuyButtonDone => _buyButtonDone;
        public string ItemsCounterTextFormat => _itemsCounterTextFormat;
        public string FreePriceText => _freePriceText;
    }
}
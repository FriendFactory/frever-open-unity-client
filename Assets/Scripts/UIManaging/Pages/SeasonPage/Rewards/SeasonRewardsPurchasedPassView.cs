using Abstract;
using UIManaging.Popups.Store;
using UnityEngine;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsPurchasedPassView : BaseContextDataView<SeasonRewardsPurchasedPremiumPassModel>
    {
        [SerializeField] private PurchasedPassView _purchasedPassView;
        
        protected override void OnInitialized()
        {
            _purchasedPassView.Init(ContextData.OnPurchasedPassClicked);
        }
    }
}
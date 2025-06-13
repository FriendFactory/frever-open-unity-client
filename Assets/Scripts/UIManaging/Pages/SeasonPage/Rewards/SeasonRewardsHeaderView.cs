using Abstract;
using Modules.InAppPurchasing;
using UIManaging.Popups.Store.SeasonPassProposal;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsHeaderView : BaseContextDataView<SeasonRewardsPremiumPassModel>
    {
        [SerializeField] private SeasonPassItemView _premiumPanel;
        [Inject] private IIAPManager _iapManager;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _premiumPanel.Initialize(_iapManager.GetSeasonPassProduct());
            _premiumPanel.OnSuccessfulPassPurchase += ContextData.OnSuccessfulPassPurchase;
        }
    }
}
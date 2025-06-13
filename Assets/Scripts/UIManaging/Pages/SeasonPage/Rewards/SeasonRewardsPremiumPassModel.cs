using System;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsPremiumPassModel : SeasonRewardsItemModel
    {
        public Action OnSuccessfulPassPurchase { get; set; }
    }

    internal sealed class RewardTabsModel : SeasonRewardsItemModel
    {
    }

    internal sealed class SeasonRewardsPurchasedPremiumPassModel : SeasonRewardsItemModel
    {
        public Action OnPurchasedPassClicked { get; set; }
    }
}
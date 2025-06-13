using System.Collections.Generic;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRepeatableBonusesRewardsLevelModel : SeasonRewardsItemModel
    {
        public List<SeasonRepeatableBonusRewardsLevelModel> BonusesCanBeClaimed { get; }
        public SeasonRepeatableBonusRewardsLevelModel NextLockedBonus { get; }
        public bool IsBonusesLocked { get; }

        public SeasonRepeatableBonusesRewardsLevelModel(List<SeasonRepeatableBonusRewardsLevelModel> bonusesCanBeClaimed, SeasonRepeatableBonusRewardsLevelModel nextLockedBonus, bool isBonusesLocked)
        {
            BonusesCanBeClaimed = bonusesCanBeClaimed;
            NextLockedBonus = nextLockedBonus;
            IsBonusesLocked = isBonusesLocked;
        }
    }
}
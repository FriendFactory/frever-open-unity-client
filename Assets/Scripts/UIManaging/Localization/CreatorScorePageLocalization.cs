using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    public class CreatorScorePageLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString[] _creatorRanks;
        [Space]
        [SerializeField] private LocalizedString _levelUpHeader;
        [SerializeField] private LocalizedString _rewardClaimedHeader;
        [SerializeField] private LocalizedString _bonusRewardTitle;
        [SerializeField] private LocalizedString _rewardUnlockRequirementFormat;
        
        [SerializeField] private LocalizedString _rewardNotAvailableMessage;
        [SerializeField] private LocalizedString _rewardClaimFailedPopupTitle;
        [SerializeField] private LocalizedString _rewardClaimFailedCloseButton;
        
        public string GetRankNameLocalized(int rank) => _creatorRanks[rank];
        
        public string LevelUpHeader => _levelUpHeader;
        public string  RewardClaimedHeader => _rewardClaimedHeader;
        public string  BonusRewardTitle => _bonusRewardTitle;
        public string RewardUnlockRequirementFormat => _rewardUnlockRequirementFormat;
        public string RewardNotAvailableMessage => _rewardNotAvailableMessage;
        public string RewardClaimFailedPopupTitle => _rewardClaimFailedPopupTitle;
        public string RewardClaimFailedCloseButton => _rewardClaimFailedCloseButton;
    }
}
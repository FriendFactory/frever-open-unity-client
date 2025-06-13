using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/SeasonPageLocalization", fileName = "SeasonPageLocalization")]
    public class SeasonPageLocalization: ScriptableObject
    {
        [SerializeField] private LocalizedString _seasonExploreDescription;
        [SerializeField] private LocalizedString _seasonTimeLeft;
        [SerializeField] private LocalizedString _seasonRewardClaimButtonText;
        
        [Header("Reward Preview")] 
        [SerializeField] private LocalizedString _rewardPreviewClaimedLabelLocked;
        [SerializeField] private LocalizedString _rewardPreviewClaimedLabelUnlocked;
        [SerializeField] private LocalizedString _rewardPreviewClaimedStatusLocked;
        [SerializeField] private LocalizedString _rewardPreviewClaimedStatusUnlockedAsset;
        [SerializeField] private LocalizedString _rewardPreviewClaimedStatusUnlockedReward;
        [SerializeField] private LocalizedString _rewardPreviewUnclaimedLabel;
        [SerializeField] private LocalizedString _rewardPreviewUnclaimedStatusLocked;
        [SerializeField] private LocalizedString _rewardPreviewUnclaimedStatusUnlocked;
        
        [Header("Asset Claimed")] 
        [SerializeField] private LocalizedString _assetClaimedSnackbarTitle; 
        [SerializeField] private LocalizedString _assetClaimedSnackbarDescription;
        [SerializeField] private LocalizedString _assetClaimedTier;

        public string SeasonExploreDescription => _seasonExploreDescription;
        public string SeasonTimeLeft => _seasonTimeLeft;
        public string SeasonRewardClaimButtonText => _seasonRewardClaimButtonText;
        public string RewardPreviewClaimedLabelLocked => _rewardPreviewClaimedLabelLocked;
        public string RewardPreviewClaimedLabelUnlocked => _rewardPreviewClaimedLabelUnlocked;
        public string RewardPreviewClaimedStatusLocked => _rewardPreviewClaimedStatusLocked;
        public string RewardPreviewClaimedStatusUnlockedAsset => _rewardPreviewClaimedStatusUnlockedAsset;
        public string RewardPreviewClaimedStatusUnlockedReward => _rewardPreviewClaimedStatusUnlockedReward;
        public string RewardPreviewUnclaimedLabel => _rewardPreviewUnclaimedLabel;
        public string RewardPreviewUnclaimedStatusLocked => _rewardPreviewUnclaimedStatusLocked;
        public string RewardPreviewUnclaimedStatusUnlocked => _rewardPreviewUnclaimedStatusUnlocked;
        public string AssetClaimedSnackbarTitle => _assetClaimedSnackbarTitle; 
        public string AssetClaimedSnackbarDescription => _assetClaimedSnackbarDescription; 
        public string AssetClaimedTier => _assetClaimedTier; 
        
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace UIManaging.Pages.SeasonPage
{
    [UsedImplicitly]
    public class SeasonRewardsHelper
    {
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private LocalUserDataHolder _userData;

        public async Task<int> IsClaimableRewardAvailable(bool forceUpdate = false)
        {
            if (forceUpdate)
            {
                await _userData.RefreshUserInfoAsync();
            }

            return IsLevelRewardAvailable();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        private int IsLevelRewardAvailable()
        {
            var season = _dataFetcher.CurrentSeason;
            if (season == null) return 0;

            var levels = season.Levels;
            var currentLevel = _userData.CurrentLevel;
            var claimedRewards = _userData.ClaimedRewards;
            var hasPremiumPass = _userData.HasPremiumPass;

            var count = 0;
            foreach (var level in levels)
            {
                if (level.Level > currentLevel) break;
                if (level.Rewards == null) continue;
                
                foreach (var reward in level.Rewards)
                {
                    if (reward.IsPremium && !hasPremiumPass) continue;
                    var isClaimed = claimedRewards.Any(id => id == reward.Id);
                    if (!isClaimed) count++;
                }
            }

            return count;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
        private bool IsLikesRewardAvailable()
        {
            var season = _dataFetcher.CurrentSeason;
            if (season == null) return false;

            var quests = season.Quests.OrderBy(quest => quest.Value).ToArray();

            foreach (var quest in quests)
            {
                if (_userData.LevelingProgress.LikesReceivedThisSeason < quest.Value) break;

                var rewards = quest.Rewards?.Length > 0 ? quest.Rewards : null;
                if (rewards is null) break;

                var reward = rewards.First();
                var claimed = reward != null && (_userData.ClaimedRewards?.Contains(reward.Id) ?? false);
                if (claimed) break;

                return true;
            }

            return false;
        }
    }
}
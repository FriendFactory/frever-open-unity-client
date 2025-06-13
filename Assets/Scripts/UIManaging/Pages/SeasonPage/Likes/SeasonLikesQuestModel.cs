using System;
using Bridge.Models.ClientServer.Gamification;

namespace UIManaging.Pages.SeasonPage.Likes
{
    internal sealed class SeasonLikesQuestModel : SeasonLikesItemModel
    {
        public long Id { get; }
        public bool Claimed { get; private set; }
        public SeasonReward Reward { get; }
        public int CurrentUserLikes { get; set; }
        public int CurrentQuestLikes { get; }
        public int? PreviousQuestLikes { get; }
        public int? NextQuestLikes { get; }
        public string Title { get; }

        public event Action<long> OnClaimReward;
        public event Action OnRewardClaimed;
        public event Action OnRewardFailed;

        public SeasonLikesQuestModel(long id, bool claimed, SeasonReward reward, int currentUserLikes, int currentQuestLikes, int? previousQuestLikes, int? nextQuestLikes, string title)
        {
            Id = id;
            Claimed = claimed;
            Reward = reward;
            CurrentUserLikes = currentUserLikes;
            CurrentQuestLikes = currentQuestLikes;
            PreviousQuestLikes = previousQuestLikes;
            NextQuestLikes = nextQuestLikes;
            Title = title;
        }

        public void ClaimReward()
        {
            OnClaimReward?.Invoke(Id);
        }

        public void ConfirmClaim()
        {
            Claimed = true;
            OnRewardClaimed?.Invoke();
        }

        public void FailClaim()
        {
            OnRewardFailed?.Invoke();
        }
    }
}
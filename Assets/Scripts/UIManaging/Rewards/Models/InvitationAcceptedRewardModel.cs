using System;

namespace UIManaging.Rewards.Models
{
    public class InvitationAcceptedRewardModel
    {
        public long Id { get; }
        public string NickName { get; }
        public int SoftCurrency { get; }

        public event Action<InvitationAcceptedRewardModel> OnRewardClaimed;

        public InvitationAcceptedRewardModel(long id, string nickName, int softCurrency)
        {
            Id = id;
            NickName = nickName;
            SoftCurrency = softCurrency;
        }

        public void ClaimReward()
        {
            OnRewardClaimed?.Invoke(this);
        }
    }
}
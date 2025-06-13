using Abstract;
using Bridge;
using UIManaging.Rewards.Models;
using UnityEngine;
using Zenject;

namespace UIManaging.Rewards.Invitation.Popups
{
    internal class ClaimAndFollowButton: BaseContextDataButton<InvitationAcceptedRewardModel>
    {
        [Inject] private IBridge _bridge;
        
        protected override async void OnUIInteracted()
        {
            var claimRewardTask = await _bridge.ClaimRewardFromInvitedUser(ContextData.Id);
            var startFollowTask = await _bridge.StartFollow(ContextData.Id);
            
            if (claimRewardTask.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to claim reward # {claimRewardTask.ErrorMessage}");
                return;
            }

            if (startFollowTask.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to start follow user # {claimRewardTask.ErrorMessage}");
            }
            
            ContextData.ClaimReward();
        }

        protected override void OnInitialized() { }
    }
}
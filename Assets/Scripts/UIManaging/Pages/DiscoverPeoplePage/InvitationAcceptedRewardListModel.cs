using System.Collections.Generic;
using System.Linq;
using Modules.DeepLinking;
using UIManaging.Rewards.Models;
using UnityEngine;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal sealed class InvitationAcceptedRewardListModel
    {
        private List<InvitationAcceptedRewardModel> _models;

        public InvitationAcceptedRewardListModel(InvitationCodeModel invitationCodeModel)
        {
            InvitationCodeModel = invitationCodeModel;
            _models = new List<InvitationAcceptedRewardModel>();
        }

        public InvitationCodeModel InvitationCodeModel { get; }
        public IReadOnlyList<InvitationAcceptedRewardModel> Models => _models;
        
        public void Initialize()
        {
            if (InvitationCodeModel.InvitationCode?.InviteGroups == null) return;
            
            _models = InvitationCodeModel.InvitationCode.InviteGroups
                        .Select(invitee => new InvitationAcceptedRewardModel( invitee.Id, invitee.NickName, InvitationCodeModel.InvitationCode.SoftCurrency))
                        .ToList();
        }

        public void RemoveReward(InvitationAcceptedRewardModel rewardModel)
        {
            if (!_models.Remove(rewardModel))
            {
                Debug.LogError($"[{GetType().Name}] Failed to remove reward # {rewardModel.Id}");
            }
        }
    }
}
using System;
using Bridge.Models.ClientServer.CreatorScore;
using UnityEngine;

namespace UIManaging.Pages.CreatorScore
{
    public class CreatorBadgeRewardModel
    {
        public CreatorBadge Badge { get; set; }
        public bool CanBeClaimed { get; set; }
        public bool IsMilestone { get; set; }
        public Sprite BadgeSprite { get; set; }

        public Action<CreatorBadgeRewardItemView, CreatorBadge> ClaimReward;
    }
}
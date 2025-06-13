using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.CreatorScore;
using UnityEngine;

namespace UIManaging.PopupSystem.Configurations
{
    public class CreatorScoreLevelUpPopupConfiguration : PopupConfiguration
    {
        public int Milestone { get; }
        public int NextMilestone { get; }
        public int NextLevel { get; }
        public Sprite BadgeSprite { get; }
        public IEnumerable<CreatorBadgeReward> Rewards { get; }

        public CreatorScoreLevelUpPopupConfiguration(int milestone, int nextMilestone, int nextLevel, Sprite badgeSprite,
            IEnumerable<CreatorBadgeReward> rewards, Action<object> onClose)
            : base(PopupType.CreatorScoreLevelUpPopup, onClose)
        {
            Milestone = milestone;
            NextMilestone = nextMilestone;
            NextLevel = nextLevel;
            BadgeSprite = badgeSprite;
            Rewards = rewards;
        }
    }
}
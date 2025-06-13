using System;
using System.Linq;
using Bridge.Models.ClientServer.Gamification;
using UnityEngine;

namespace Extensions
{
    public static class SeasonExtensions
    {
        public static SeasonReward[] GetPremiumLevelRewards(this CurrentSeason season)
        {
            return season.Levels.Where(x => x.Rewards != null).SelectMany(x=> x.Rewards).Where(x=>x.IsPremium).ToArray();
        }
    }
}
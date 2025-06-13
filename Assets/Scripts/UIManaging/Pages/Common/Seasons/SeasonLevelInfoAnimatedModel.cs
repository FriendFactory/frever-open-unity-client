using System.Linq;
using Bridge.Models.ClientServer.Gamification;
using UnityEngine;

namespace UIManaging.Pages.Common.Seasons
{
    public class SeasonLevelInfoAnimatedModel : ISeasonLevelInfoModel
    {
        public float StartValue { get; private set; }
        public float EndValue { get; private set; }
        public int MaxValue { get; private set; }
        public int Level { get; private set; }
        
        private readonly CurrentSeason _currentSeason;
        private int _currentXp;
        private int _remainingXpReward;

        public SeasonLevelInfoAnimatedModel(int currentXp, int xpReward, CurrentSeason currentSeason, int levelBeforeTask)
        {
            _currentSeason = currentSeason;

            _currentXp = currentXp;
            _remainingXpReward = xpReward;

            Level = levelBeforeTask;
        }

        public void UpdateRemainingXp(int xp)
        {
            _remainingXpReward += xp;
        }

        public bool UpdateForNextCycle()
        {
            if (_currentSeason.Levels.Length <= Level)
            {
                MaxValue = _currentSeason.Levels.Last().XpRequired;
                StartValue = EndValue = MaxValue;
                return false;
            }

            var currentLevelXpRequirement = Level == 0 ? 0 : _currentSeason.Levels[Level - 1].XpRequired;
            var nextLevelXpRequirement = _currentSeason.Levels[Level].XpRequired;
            var gainedXp = Mathf.Min(nextLevelXpRequirement - _currentXp, _remainingXpReward);

            MaxValue = nextLevelXpRequirement - currentLevelXpRequirement;
            StartValue = Mathf.Clamp(_currentXp - currentLevelXpRequirement, 0, MaxValue);

            _currentXp += gainedXp;
            _remainingXpReward -= gainedXp;

            EndValue = Mathf.Clamp(_currentXp - currentLevelXpRequirement, 0, MaxValue);

            Level++;

            // Reached exactly required xp, run one more cycle to update view to current level
            if (_remainingXpReward == 0 && _currentXp == nextLevelXpRequirement) return true;

            return _remainingXpReward > 0;
        }
    }
}
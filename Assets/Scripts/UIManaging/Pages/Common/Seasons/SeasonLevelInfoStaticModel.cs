using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace UIManaging.Pages.Common.Seasons
{
    public class SeasonLevelInfoStaticModel: ISeasonLevelInfoModel
    {
        public float StartValue { get; private set; }
        public float EndValue { get; private set; }
        public int MaxValue { get; private set; }
        public int Level { get; private set; }

        private readonly LocalUserDataHolder _dataHolder;
        
        public SeasonLevelInfoStaticModel(LocalUserDataHolder dataHolder)
        {
            _dataHolder = dataHolder;
            
            Level = _dataHolder.LevelingProgress.Xp.CurrentLevel?.Level ?? 0;
        }
        
        public bool UpdateForNextCycle()
        {
            var xpData = _dataHolder.LevelingProgress.Xp;

            var currentLevelXp = xpData.NextLevel == null 
                ? xpData.PreviousLevel?.Xp ?? 0 
                : xpData.CurrentLevel?.Xp ?? 0;
            var nextLevelXp = xpData.NextLevel?.Xp ?? xpData.CurrentLevel?.Xp ?? 0;

            MaxValue = nextLevelXp - currentLevelXp;
            StartValue = EndValue = Mathf.Clamp(xpData.Xp - currentLevelXp, 0, MaxValue);
            Level = xpData.CurrentLevel?.Level ?? 0;
            
            return false;
        }
    }
}
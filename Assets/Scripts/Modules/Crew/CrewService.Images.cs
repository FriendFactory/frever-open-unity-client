using UnityEngine;

namespace Modules.Crew
{
    public sealed partial class CrewService
    {
        private const string TH_BACKGROUND_KEY = "TrophyHuntBackground";
        private const string TH_REWARD_BACKGROUND_KEY = "TrophyHuntRewardsBackground";
        private const string CREW_BALL_KEY = "CrewBallBackground";
        private const string CREW_BANNER_KEY = "CrewBannerBackground";

        public void PrefetchBackgrounds()
        {
            PrefetchBackgroundImage(TH_BACKGROUND_KEY);
            PrefetchBackgroundImage(TH_REWARD_BACKGROUND_KEY);
            PrefetchBackgroundImage(CREW_BANNER_KEY);
            PrefetchBackgroundImage(CREW_BALL_KEY);
        }

        public Texture2D GetTrophyHuntBackground()
        {
            return GetBackgroundFromCache(TH_BACKGROUND_KEY);
        }

        public Texture2D GetTrophyHuntRewardsBackground()
        {
            return GetBackgroundFromCache(TH_REWARD_BACKGROUND_KEY);
        }

        public Texture2D GetCrewBall()
        {
            return GetBackgroundFromCache(CREW_BALL_KEY);
        }

        public Texture2D GetCrewBanner()
        {
            return GetBackgroundFromCache(CREW_BANNER_KEY);
        }

        private async void PrefetchBackgroundImage(string key)
        {
            var result = await _bridge.FetchImageAsync(key);
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);
        }

        private Texture2D GetBackgroundFromCache(string key)
        {
            var result = _bridge.GetImageFromCache(key);
            if (result.IsError) Debug.LogError(result.ErrorMessage);

            return result.Model;
        }
    }
}
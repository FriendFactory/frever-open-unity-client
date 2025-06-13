using System;
using System.Collections.Generic;
using Extensions;
using UIManaging.Pages.Crews.TrophyHunt;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace UIManaging.Pages.Crews
{
    public class TrophyHuntRewardsProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _progress;
        [SerializeField] private TrophyHuntProgressMilestoneView _milestonePrefab;

        public void Init(int[] steps, int stepSize, int currentScore)
        {
            var stepCounter  = 0;
            var offsetY = 0f;
            var lastUnlockedIndex = 0;
            var sizeDeltaY = 0f;
            
            foreach (var step in steps)
            {
                var milestoneObject = Instantiate(_milestonePrefab, transform);
                var milestoneRect = (RectTransform)milestoneObject.transform;
                milestoneObject.Init(step);

                offsetY = stepSize * (stepCounter + 0.5f);
                milestoneRect.anchoredPosition = new Vector2(0, - offsetY);

                if (step <= currentScore)
                {
                    lastUnlockedIndex = stepCounter;
                    sizeDeltaY = offsetY;
                }
                
                ++stepCounter;
            }

            if (lastUnlockedIndex + 1 < steps.Length)
            {
                var pointsToNextRewardTotal = steps[lastUnlockedIndex + 1] - steps[lastUnlockedIndex];
                var pointsToNextRewardCurrent = steps[lastUnlockedIndex + 1] - currentScore;
                sizeDeltaY += (1f - (float)pointsToNextRewardCurrent / pointsToNextRewardTotal) * stepSize;
            }
            
            _progress.sizeDelta = new Vector2(_progress.sizeDelta.x, sizeDeltaY);

            _background.sizeDelta = new Vector2(0, - stepSize / 2f);
        }
    }
}
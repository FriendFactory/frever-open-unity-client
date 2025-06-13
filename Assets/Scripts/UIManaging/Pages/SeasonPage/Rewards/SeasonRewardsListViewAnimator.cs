using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UnityEngine;
using Tween = DG.Tweening.Tween;

namespace UIManaging.Pages.SeasonPage
{
    public class SeasonRewardsListViewAnimator: MonoBehaviour
    {
        [SerializeField] private SeasonRewardsListView _listView;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _ease = Ease.OutCubic;

        private Tween _scrollDownTween;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize()
        {
            var normalizedScrollPosition = CalculateNormalizedPosition();

            _scrollDownTween = _enhancedScroller.DOScroll(normalizedScrollPosition, _duration)
                                                .SetAutoKill(false)
                                                .SetEase(_ease)
                                                .Pause();
        }

        public void ScrollDown()
        {
            _scrollDownTween.Play();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private float CalculateNormalizedPosition()
        {
            var currentLevelIndex = _listView.ContextData.CurrentLevelIndex;

            // do not scroll when user's level is 1
            currentLevelIndex = currentLevelIndex > 1 ? currentLevelIndex : 0;

            var levelsCount = _listView.ContextData.Items.Count;

            return 1f - Mathf.Clamp01(currentLevelIndex / Mathf.Max(1f, levelsCount - 3));
        }
    }
}
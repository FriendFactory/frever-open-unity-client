using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UIManaging.Pages.SeasonPage.Likes;
using UnityEngine;
using Tween = DG.Tweening.Tween;

namespace UIManaging.Pages.SeasonPage
{
    public class SeasonLikesListViewAnimator : MonoBehaviour
    {
        [SerializeField] private SeasonLikesListView _listView;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _ease = Ease.OutCubic;

        private Tween _scrollDownTween;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize()
        {
            var normalizePosition = CalculateNormalizedPosition();

            _scrollDownTween = _enhancedScroller.DOScroll(normalizePosition, _duration)
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
            var likesMilestoneCount = _listView.ContextData.Items.Length;
            var currentIndex = _listView.ContextData.StartIndex;

            if (likesMilestoneCount == 0) return 1;

            return 1f - (float) currentIndex / likesMilestoneCount;
        }
    }
}
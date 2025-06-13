using Common;
using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VotingResult
{
    internal sealed class VotingResultsViewAnimator: MonoBehaviour
    {
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _delay = 1f;
        [SerializeField] private Ease _ease = Ease.OutCubic;
        [SerializeField] private ScrollRect _scrollRect;

        public void ScrollTo(RectTransform item)
        {
            CoroutineSource.Instance.ExecuteWithDelay(_delay, () =>
            {
                if (item == null)
                {
                    return;
                }
                
                var normalized = _scrollRect.CalculateFocusedScrollPosition(item);
                _scrollRect.DOVerticalScroll(normalized.y, _duration)
                           .SetAutoKill(false)
                           .SetEase(_ease);
            });
        }
    }
}
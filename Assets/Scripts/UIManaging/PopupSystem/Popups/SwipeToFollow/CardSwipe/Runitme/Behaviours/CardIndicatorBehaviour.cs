using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Behaviours
{
    public class CardIndicatorBehaviour : CardBaseBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup rightSwipeCanvasGroup;
        [SerializeField] private CanvasGroup leftSwipeCanvasGroup;
        [SerializeField] private CanvasGroup upSwipeCanvasGroup;
        [SerializeField] private CanvasGroup downSwipeCanvasGroup;


        private void Update()
        {
            var deltaNormal = Card.GetDeltaNormal();
            var currentSwipeDirection = Card.GetCurrentSwipeDirection(false);

            var right = 0f;
            var left = 0f;
            var up = 0f;
            var down = 0f;
                
                
            switch (currentSwipeDirection)
            {
                case Core.SwipeDirection.Right:
                    right = deltaNormal;
                    break;
                case Core.SwipeDirection.Left:
                    left = deltaNormal;
                    break;
                case Core.SwipeDirection.Up:
                    up = deltaNormal;
                    break;
                case Core.SwipeDirection.Down:
                    down = deltaNormal;
                    break;
            }
                
            if(rightSwipeCanvasGroup != null) rightSwipeCanvasGroup.alpha = right;
            if(leftSwipeCanvasGroup != null) leftSwipeCanvasGroup.alpha = left;
            if(upSwipeCanvasGroup != null) upSwipeCanvasGroup.alpha = up;
            if(downSwipeCanvasGroup != null) downSwipeCanvasGroup.alpha = down;
        }
    }
}
using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Behaviours
{
    public class CardFadeColorBehaviour: CardBaseBehaviour
    {
        [SerializeField] private Graphic _target;
        [SerializeField] private Color _leftSwipeColor = Color.red;
        [SerializeField] private Color _rightSwipeColor = Color.green;
        
        private Color _initialColor;

        private void Awake()
        {
            _initialColor = _target.color;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _target.color = _initialColor;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _target.color = _initialColor;
        }

        private void Update()
        {
            var deltaNormal = Card.GetDeltaNormal();
            var currentSwipeDirection = Card.GetCurrentSwipeDirection(false);
            var targetColor = currentSwipeDirection == SwipeDirection.Right ? _rightSwipeColor : _leftSwipeColor;
            
            _target.color = Color.Lerp(_initialColor, targetColor, deltaNormal);
        }
    }
}
using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Behaviours;
using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.States
{
    internal sealed class UserCardInDeckViewState: UserCardViewStateBase
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private CardSwipeBehaviour _cardSwipeBehaviour;
        [SerializeField] private CardRotationBehaviour _cardRotationBehaviour;
        [SerializeField] private CardFadeColorBehaviour _cardFadeColorBehaviour;
        [Header("Fade Effect")]
        [SerializeField] private Graphic _overlay;
        [SerializeField] private Color _overlayColor = Color.gray;
        
        private Color _initialOverlayColor;

        private void Awake()
        {
            _canvasGroup.interactable = false;
            
            // we need to disable all behaviours here to prevent any updates inside Update loop
            _cardSwipeBehaviour.enabled = false;
            _cardRotationBehaviour.enabled = false;
            _cardFadeColorBehaviour.enabled = false;
            
            _initialOverlayColor = _overlay.color;
        }

        protected override void OnEnter()
        {
            _overlay.color = _overlayColor;
        }

        protected override void OnExit()
        {
            _canvasGroup.interactable = true;
            
            _overlay.color = _initialOverlayColor;
        }
    }
}
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Behaviours;
using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.States
{
    internal sealed class UserCardOnTopViewState: UserCardViewStateBase
    {
        [SerializeField] private CardSwipeBehaviour _cardSwipeBehaviour;
        [SerializeField] private CardRotationBehaviour _cardRotationBehaviour;
        [SerializeField] private CardFadeColorBehaviour _cardFadeColorBehaviour;

        [Inject] private FollowersManager _followersManager;

        protected override void OnEnter()
        {
            _cardSwipeBehaviour.enabled = true;
            _cardRotationBehaviour.enabled = true;
            _cardFadeColorBehaviour.enabled = true;

            _cardSwipeBehaviour.OnCardSwiped += OnCardSwiped;
        }

        protected override void OnExit()
        {
            _cardSwipeBehaviour.enabled = false;
            _cardRotationBehaviour.enabled = false;
            _cardFadeColorBehaviour.enabled = false;
            
            _cardSwipeBehaviour.OnCardSwiped -= OnCardSwiped;
        }

        private void OnCardSwiped(CardSwipe.Core.SwipeDirection swipeDirection)
        {
            if (swipeDirection is CardSwipe.Core.SwipeDirection.None)  return;
            
            ContextData.SwipeDirection = swipeDirection == CardSwipe.Core.SwipeDirection.Left
                ? SwipeDirection.Left
                : SwipeDirection.Right;

            if (ContextData.SwipeDirection == SwipeDirection.Right)
            {
                _followersManager.FollowUser(ContextData.Profile.MainGroupId);
            }
            
            ContextData.Fire(UserCardTrigger.ManualSwipe);
        }
    }
}
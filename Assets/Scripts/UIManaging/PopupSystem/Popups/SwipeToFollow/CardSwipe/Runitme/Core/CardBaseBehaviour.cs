using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core
{
    public abstract class CardBaseBehaviour : MonoBehaviour
    {
        protected CardSwipeBehaviour Card;

        protected virtual void OnEnable()
        {
            Card = GetComponent<CardSwipeBehaviour>();
            Card.OnResetCardStarted += OnResetCardStarted;
            Card.OnResetCardCompleted += OnResetCardCompleted;
            Card.OnCardSwiped += OnCardSwiped;
        }

        protected virtual void OnDisable()
        {
            if (Card == null) return;
            Card.OnResetCardStarted -= OnResetCardStarted;
            Card.OnResetCardCompleted -= OnResetCardCompleted;
            Card.OnCardSwiped -= OnCardSwiped;
        }

        protected virtual void OnResetCardStarted()
        {
            
        }
        
        protected virtual void OnResetCardCompleted()
        {
            
        }
        
        protected virtual void OnCardSwiped(SwipeDirection direction)
        {
            
        }
    }
}
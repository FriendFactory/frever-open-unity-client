using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Behaviours
{
    public class CardRotationBehaviour : CardBaseBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float rotation = 30;
        [SerializeField] private float speed = 10;
        
        private void Update()
        {
            //Check if the Card is dragging or is Animating
            if (Card.IsDragging || Card.IsAnimating)
            {
                var rot = rotation;
                if (Card.RectTransform.anchoredPosition.x > 0) rot = -rotation;

                //Apply the Rotation
                Card.RectTransform.rotation = 
                    Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(0,0,rot), Card.GetDeltaNormal(SwipeAlignment.Horizontal));
            }
            else
            {
                //reset the rotation in case there is no Drag or the card is not Animating
                Card.RectTransform.rotation = Quaternion.Slerp(Card.RectTransform.rotation,Quaternion.identity, Time.deltaTime * speed);
            }
        }

        //Reset the Card rotating when the Swipe is completed
        protected override void OnResetCardCompleted()
        {
            Card.RectTransform.rotation = Quaternion.identity;
        }
    }
}
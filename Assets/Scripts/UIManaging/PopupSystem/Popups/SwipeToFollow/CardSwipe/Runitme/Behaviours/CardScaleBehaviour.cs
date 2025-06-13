using UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Behaviours
{
    public class CardScaleBehaviour : CardBaseBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float normalScale = 1;
        [SerializeField] private float holdScale = 1.1f;
        
        private void Update()
        {
            //Calculate the Scale
            var newScale = Vector3.Lerp(Vector3.one * normalScale,Vector3.one * holdScale,Card.GetDeltaNormal());

            if (Card.IsDragging || Card.IsAnimating)
            {
                //Apply the Scale
                Card.transform.localScale = Vector3.Lerp(Card.transform.localScale,newScale,Time.deltaTime * 10);
            }
            else
            {
                //Reset the Card Scale when the Card is not Animating or in the drag state
                Card.transform.localScale = Vector3.Lerp(Card.transform.localScale,
                    Vector3.one * normalScale, Time.deltaTime * 10);
            }
        }

        //Reset the Card Scale when the Swipe is completed
        protected override void OnResetCardCompleted()
        {
            Card.transform.localScale = Vector3.one * normalScale;
        }
    }
}
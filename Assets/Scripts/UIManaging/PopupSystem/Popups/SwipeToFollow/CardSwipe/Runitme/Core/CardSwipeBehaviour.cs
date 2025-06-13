using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.CardSwipe.Core
{
    public sealed class CardSwipeBehaviour : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        //-------------------- Events ------------------------
        public event System.Action<SwipeDirection> OnCardSwiped;
        public event System.Action OnResetCardStarted;
        public event System.Action OnResetCardCompleted;
        
        [Header("Movements")] 
        [SerializeField,Tooltip("if checked the Card will be moved using the lerp method")]
        private bool useLerp = true;
        [SerializeField,Tooltip("Lerp Speed higher means the card will move quicker")]
        private float lerp = 10;
        [Header("Swipe Animation")] 
        [SerializeField,Tooltip("the duration of the final animation when the card completed the swipe")]
        private float completeSwipeDuration = .5f;
        [SerializeField,Tooltip("the speed of the final animation")]
        private float completeSwipeSpeed = 250;
        [Header("Settings")] 
        [SerializeField,Tooltip("Offset of the Card this is helpful in case you are planning on adjusting the pivot")]
        private Vector2 offset = new Vector2(0,0);
        [SerializeField,Tooltip("The Card will ignore the Swipe if the distance is below this value")]
        private float minDragDistance = 300;
        [SerializeField,Tooltip("The Card will ignore the quick Swipe if the speed is below this value")]
        private float minDragSpeed = 1500;
        [Header("Alignment")] 
        [SerializeField,Tooltip("Check this if you want the card to move horizontally")]
        private bool swipeHorizontal = true;
        [SerializeField,Tooltip("Check this if you want the card to move vertically")]
        private bool swipeVertical = true;

        private float _swipeStartTime;
        
        //-------------------- Properties ------------------------
        public RectTransform RectTransform { get; protected set; }
        public bool IsDragging { get; protected set; }
        public bool IsAnimating { get; protected set; }
        
        //-------------------- Getter ------------------------
        public float MinDragDistance
        {
            get => minDragDistance;
            set => minDragDistance = value;
        }
        
        public float MinDragSpeed
        {
            get => minDragSpeed;
            set => minDragSpeed = value;
        }

        //-------------------- Fields ------------------------
        private Vector2 StartAnchorPosition;
        private Vector2 NewPos;

        //-------------------- Unity ------------------------
        private void Awake()
        {
            //Get the needed references
            RectTransform = GetComponent<RectTransform>();
            //Check if there is an EndlessCardManager in the Parent
            StartAnchorPosition = RectTransform.anchoredPosition;
        }

        private void Update()
        {
            UpdateDrag();
        }

        //-------------------- Drag ------------------------
        
        //Method called by Unity When the Drag Started
        public void OnBeginDrag(PointerEventData eventData)
        {
            StartAnchorPosition = RectTransform.anchoredPosition;
            _swipeStartTime = Time.time;
            IsDragging = true;
        }

        //Method called by Unity When the Drag Updated
        public void OnDrag(PointerEventData eventData)
        {
            var mouseDelta = eventData.position - eventData.pressPosition;
            // mouseDelta /= EndlessCardsManager.Canvas.scaleFactor;

            if (!swipeHorizontal)
            {
                mouseDelta.x = 0;
            }

            if (!swipeVertical)
            {
                mouseDelta.y = 0;
            }
            
            NewPos = StartAnchorPosition + mouseDelta;
        }

        //Method called by Unity When the Drag Completed
        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;

            var currentSwipeDirection = GetCurrentSwipeDirection();
            
            if (currentSwipeDirection == SwipeDirection.None) return;

            StartCoroutine(CompleteSwipe(currentSwipeDirection, () => OnCardSwiped?.Invoke(currentSwipeDirection)));
        }
        
        //-------------------- Control Card ------------------------
        
        /// <summary>
        /// Reset The card after registering the swipe
        /// </summary>
        /// <param name="swipeDirection">swipe direction</param>
        public void ResetCard(SwipeDirection swipeDirection)
        {
            if (swipeDirection != SwipeDirection.None)
            {
                OnResetCardStarted?.Invoke();
                StopAllCoroutines();
                StartCoroutine(CompleteSwipe(swipeDirection, () =>
                {
                    OnResetCardCompleted?.Invoke();
                    RectTransform.anchoredPosition = offset;
                    transform.SetAsFirstSibling();
                    // EndlessCardsManager.ResetCard(this);
                }));
            }
        }
        
        //-------------------- Control ------------------------

        private void UpdateDrag()
        {
            if (!IsDragging)
            {
                if (!IsAnimating)
                {
                    RectTransform.anchoredPosition = Vector2.Lerp(RectTransform.anchoredPosition,Vector2.zero + offset, Time.deltaTime * lerp);
                }
                return;
            }
            if (useLerp) RectTransform.anchoredPosition = Vector3.Lerp(RectTransform.anchoredPosition,NewPos,Time.deltaTime * lerp);
            else RectTransform.anchoredPosition = NewPos;
        }

        private IEnumerator CompleteSwipe(SwipeDirection swipeDirection,Action completeCallback)
        {
            IsAnimating = true;

            var timer = 0f;

            while (timer <= completeSwipeDuration)
            {
                timer += Time.deltaTime;
                RectTransform.anchoredPosition += GetDirection() * (Time.deltaTime * completeSwipeSpeed);
                yield return null;
            }
            IsAnimating = false;
            
            completeCallback?.Invoke();
        }

        /// <summary>
        /// Calculate the current anchor delta and returns the swipe direction
        /// </summary>
        /// <param name="withNone">should check for None State</param>
        /// <returns>the final Swipe Direction</returns>
        public SwipeDirection GetCurrentSwipeDirection(bool withNone = true)
        {
            var anchoredPosition = RectTransform.anchoredPosition - StartAnchorPosition;
            var speed = anchoredPosition.magnitude / (Time.time - _swipeStartTime);
            if (withNone)
            {
                if (anchoredPosition.magnitude < minDragDistance && speed < minDragSpeed)
                {
                    return SwipeDirection.None;
                }
            }
            
            // Horizontal
            if (Mathf.Abs(anchoredPosition.x) > Mathf.Abs(anchoredPosition.y)) 
            {
                return anchoredPosition.x > 0 ?
                    //Right
                    SwipeDirection.Right :
                    //Left
                    SwipeDirection.Left;
            }

            // Vertical
            return anchoredPosition.y > 0 ?
                //Up
                SwipeDirection.Up :
                //Down
                SwipeDirection.Down;
        }

        /// <summary>
        /// Get the Direction 
        /// </summary>
        /// <returns></returns>
        public Vector2 GetDirection()
        {
            return (RectTransform.anchoredPosition - StartAnchorPosition).normalized;
        }

        public Vector2 GetAnchoredPositionDelta()
        {
            return (RectTransform.anchoredPosition - StartAnchorPosition);
        }

        public float GetDeltaNormal()
        {
            var anchoredPositionDelta = GetAnchoredPositionDelta();
            return Mathf.Abs(anchoredPositionDelta.magnitude) / MinDragDistance;
        }

        public float GetDeltaNormal(SwipeAlignment swipeAlignment)
        {
            var anchoredPositionDelta = GetAnchoredPositionDelta();
            switch (swipeAlignment)
            {
                case SwipeAlignment.Horizontal:
                    anchoredPositionDelta.y = 0;
                    break;
                case SwipeAlignment.Vertical:
                    anchoredPositionDelta.x = 0;
                    break;
            }
            return Mathf.Abs(anchoredPositionDelta.magnitude) / MinDragDistance;
        }
    }
}
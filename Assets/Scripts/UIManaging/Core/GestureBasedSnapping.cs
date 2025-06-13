using System;
using System.Collections;
using DigitalRubyShared;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Core
{
    [RequireComponent(typeof(ScrollRectEx))]
    [RequireComponent(typeof(EnhancedScroller))]
    public abstract class GestureBasedSnapping<TGesture>: MonoBehaviour where TGesture : GestureRecognizer, new()
    {
        [SerializeField] private bool _createGestureTarget = true;
        [Header("Snapping")] 
        [SerializeField] protected EnhancedScroller.TweenType _snapType = EnhancedScroller.TweenType.easeInQuad;
        [SerializeField] protected float _snapTime = 0.25f;
        
        private ScrollRectEx _scrollRect;
        private EnhancedScroller _enhancedScroller;
        private int _startScrollIndex;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<int> Snapping;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected abstract TGesture Gesture { get; }
        
        protected int LastIndex => _enhancedScroller.NumberOfCells - 1;
        protected int StartScrollIndex => _startScrollIndex;
        protected EnhancedScroller EnhancedScroller => _enhancedScroller;
        protected ScrollRectEx ScrollRect => _scrollRect;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _scrollRect = GetComponent<ScrollRectEx>();
            _scrollRect.horizontal = true;
            _scrollRect.vertical = false;
            _scrollRect.inertia = true;
            _scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
            
            _enhancedScroller = GetComponent<EnhancedScroller>();
            _enhancedScroller.snapping = false;
            _enhancedScroller.snapSkipNext = false;

            if (_createGestureTarget)
            {
                StartCoroutine(CreateGestureTargetIfNeeded());
            }
        }

        protected void OnEnable()
        {
            _scrollRect.DragBegan += OnBeginDrag;

            Gesture.StateUpdated += OnGestureStateUpdated;
        }

        protected void OnDisable()
        {
            _scrollRect.DragBegan -= OnBeginDrag;

            Gesture.StateUpdated -= OnGestureStateUpdated;
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------
        
        private void OnBeginDrag(PointerEventData eventData)
        {
            var currentPosition = _enhancedScroller.ScrollPosition + (_enhancedScroller.ScrollRectSize * Mathf.Clamp01(_enhancedScroller.snapWatchOffset));
            _startScrollIndex = _enhancedScroller.GetCellViewIndexAtPosition(currentPosition);
        }

        protected void OnSnap(int index)
        {
            Snapping?.Invoke(index);
        }
        

        protected abstract void OnGestureStateUpdated(GestureRecognizer gesture);
        
        //Create target for detecting gestures
        private IEnumerator CreateGestureTargetIfNeeded()
        {
            while (_enhancedScroller.Container == null)
            {
                yield return null;
            }
            
            var panel = new GameObject("GestureTarget", typeof(RectTransform));
            panel.transform.SetParent(transform);
            panel.transform.SetAsFirstSibling();
            
            var image = panel.AddComponent<Image>();
            image.color = new Color(1, 1, 1, 0);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1, 1f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            
            Gesture.PlatformSpecificView = panel;
        }
    }
}
using DigitalRubyShared;
using UnityEngine;

namespace UIManaging.Core
{
    [RequireComponent(typeof(PanGestureRecognizerComponentScript))]
    public class PanGestureBasedSnapping: GestureBasedSnapping<PanGestureRecognizer>
    {
        private PanGestureRecognizerComponentScript _gestureComponent;

        protected override void Awake()
        {
            _gestureComponent = GetComponent<PanGestureRecognizerComponentScript>();
            
            base.Awake();
        }

        protected override PanGestureRecognizer Gesture => _gestureComponent.Gesture;

        protected override void OnGestureStateUpdated(GestureRecognizer gesture)
        {
            if (gesture.State != GestureRecognizerState.Ended) return;
            
            OnPanGestureEnded();
        }

        private void OnPanGestureEnded()
        {
            if (EnhancedScroller.IsTweening) return;
            
            SnapClosest();
        }
        
        private void SnapClosest()
        {
            var scrollOutOfBoundsRight = ScrollRect.normalizedPosition.x > 1;
            var scrollOutOfBoundsLeft = ScrollRect.normalizedPosition.x < 0;
            if (scrollOutOfBoundsRight || scrollOutOfBoundsLeft) return;
            
            EnhancedScroller.Snap();
            OnSnap(EnhancedScroller.TargetSnapCellViewIndex);
        }
    }
}
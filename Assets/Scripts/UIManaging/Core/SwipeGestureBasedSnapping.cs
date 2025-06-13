using DigitalRubyShared;
using UnityEngine;

namespace UIManaging.Core
{
    public class SwipeGestureBasedSnapping: GestureBasedSnapping<SwipeGestureRecognizer>
    {
        private SwipeGestureRecognizerComponentScript _gestureComponent;

        protected override SwipeGestureRecognizer Gesture => _gestureComponent.Gesture;
        
        protected override void Awake()
        {
            _gestureComponent = GetComponent<SwipeGestureRecognizerComponentScript>();
            
            base.Awake();
        }

        protected override void OnGestureStateUpdated(GestureRecognizer gesture)
        {
            if (gesture.State != GestureRecognizerState.Ended) return;
            
            OnSwipeGestureEnded();
        }

        private void OnSwipeGestureEnded()
        {
            switch (Gesture.EndDirection)
            {
                case SwipeGestureRecognizerDirection.Left:
                    SnapNext();
                    break;
                case SwipeGestureRecognizerDirection.Right:
                    SnapPrevious();
                    break;
            }
        }

        private void SnapNext()
        {
            var snapIndex = Mathf.Clamp(StartScrollIndex + 1, 0, LastIndex);
            if (snapIndex == LastIndex && StartScrollIndex == LastIndex) return;

            Snap(snapIndex);
        }

        private void SnapPrevious()
        {
            var snapIndex = Mathf.Clamp(StartScrollIndex - 1, 0, LastIndex);
            if (snapIndex == 0 && StartScrollIndex == 0) return;

            Snap(snapIndex);
        }

        private void Snap(int index)
        {
            EnhancedScroller.JumpToDataIndex(index, EnhancedScroller.snapJumpToOffset, EnhancedScroller.snapCellCenterOffset, EnhancedScroller.snapUseCellSpacing, _snapType, _snapTime);
            OnSnap(index);
        }
    }
}
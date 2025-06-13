using System;
using DigitalRubyShared;

namespace Modules.InputHandling.Gestures
{
    internal abstract class InputBase
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        public abstract GestureRecognizer Gesture { get; }
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        public event Action<float> GestureExecuting;
        public event Action GestureEnd;
        public event Action GestureBegan;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected abstract float GestureMovementCalculation(GestureRecognizer gesture);
        protected void UpdateGesture(GestureRecognizer gesture)
        {
            if (gesture.GetType() != Gesture.GetType()) return;

            switch (Gesture.State)
            {
                case GestureRecognizerState.Began:
                    OnGestureBegan();
                    break;
                case GestureRecognizerState.Executing:
                    var value = GestureMovementCalculation(gesture);
                    OnGestureExecuting(value);
                    break;
                case GestureRecognizerState.Ended:
                    OnGestureEnded();
                    break;
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnGestureBegan()
        {
            GestureBegan?.Invoke();
        }
        private void OnGestureExecuting(float value)
        {
            GestureExecuting?.Invoke(value);
        }

        private void OnGestureEnded()
        {
            GestureEnd?.Invoke();
        }
        
    }
}
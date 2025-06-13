using DigitalRubyShared;

namespace Modules.InputHandling.Gestures
{
    internal sealed class ZoomInput : InputBase
    {
        private ScaleGestureRecognizer _gesture;
        public override GestureRecognizer Gesture => _gesture ?? (_gesture = new ScaleGestureRecognizer());

        public ZoomInput()
        {
            Gesture.StateUpdated += UpdateGesture;
        }
        
        protected override float GestureMovementCalculation(GestureRecognizer gesture)
        {
            var scaleGesture = gesture as ScaleGestureRecognizer;
            return GetInvertedScale(scaleGesture);
        }

        private float GetInvertedScale(ScaleGestureRecognizer gesture)
        {
            // invert the scale so that smaller scales actually zoom out and larger scales zoom in
            return 1.0f + (1.0f - gesture.ScaleMultiplier);
        }
    }
}

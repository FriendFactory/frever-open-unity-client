using DigitalRubyShared;

namespace Modules.InputHandling.Gestures
{
    internal sealed class RotationInput : InputBase
    {
        private GestureRecognizer _gesture;
        public override GestureRecognizer Gesture => _gesture ?? (_gesture = new RotateGestureRecognizer());

        public RotationInput()
        {
            Gesture.StateUpdated += UpdateGesture;
        }

        protected override float GestureMovementCalculation(GestureRecognizer gesture)
        {
            var rotationGesture = gesture as RotateGestureRecognizer;
            return rotationGesture.RotationDegreesDelta;
        }
    }
}
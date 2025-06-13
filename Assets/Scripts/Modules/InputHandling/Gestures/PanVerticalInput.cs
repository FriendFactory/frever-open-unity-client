using DigitalRubyShared;

namespace Modules.InputHandling.Gestures
{
    internal sealed class PanVerticalInput : PanInputBase
    {
        private const float INPUT_MULTIPLIER = 0.0008f;

        public PanVerticalInput(PanGestureRecognizer gesture) : base(gesture)
        {
        }
        
        protected override float GestureMovementCalculation(GestureRecognizer gesture)
        {
            var panGesture = gesture as PanGestureRecognizer;
            return -panGesture.DeltaY * INPUT_MULTIPLIER;
        }
    }
}

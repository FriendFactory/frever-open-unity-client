using DigitalRubyShared;

namespace Modules.InputHandling.Gestures
{
    internal sealed class PanHorizontalInput : PanInputBase
    {
        private const float INPUT_MULTIPLIER = 0.1f;
        
        public PanHorizontalInput(PanGestureRecognizer gesture) : base(gesture)
        {
        }
        
        protected override float GestureMovementCalculation(GestureRecognizer gesture)
        {
            var panGesture = gesture as PanGestureRecognizer;
            return panGesture.DeltaX * INPUT_MULTIPLIER;
        }
    }
}

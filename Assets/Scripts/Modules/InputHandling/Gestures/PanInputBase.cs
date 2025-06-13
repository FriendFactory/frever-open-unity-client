using DigitalRubyShared;

namespace Modules.InputHandling.Gestures
{
    internal abstract class PanInputBase : InputBase
    {
        private readonly PanGestureRecognizer _gesture;
        public override GestureRecognizer Gesture => _gesture;

        protected PanInputBase(PanGestureRecognizer gesture)
        {
            _gesture = gesture;
            _gesture.StateUpdated += UpdateGesture;
        }
    }
}

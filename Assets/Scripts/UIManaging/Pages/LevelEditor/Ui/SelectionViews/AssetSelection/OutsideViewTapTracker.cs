using System;
using DigitalRubyShared;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public interface IOutsideViewTapTracker: IDisposable
    {
        event Action TappedOutsideView;
        void Run();
        void Stop();
    }
    
    internal sealed class OutsideViewTapTracker: IOutsideViewTapTracker
    {
        private readonly TapGestureRecognizer _tapGesture;
        private readonly FingersScript _fingersScript;
        private bool _isRunning;

        public event Action TappedOutsideView;

        public OutsideViewTapTracker(FingersScript fingersScript)
        {
            _fingersScript = fingersScript;
            _tapGesture = new TapGestureRecognizer();
        }

        public void Run()
        {
            if(_isRunning) return;
            _isRunning = true;
            
            _fingersScript.AddGesture(_tapGesture);
            _tapGesture.StateUpdated += HandleStateUpdate;
        }

        public void Stop()
        {
            if(!_isRunning) return;
            _isRunning = false;
            
            _fingersScript.RemoveGesture(_tapGesture);
            _tapGesture.StateUpdated -= HandleStateUpdate;
        }
        
        public void Dispose()
        {
            Stop();
            _tapGesture?.Dispose();
        }
        
        private void HandleStateUpdate(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                TappedOutsideView?.Invoke();
            }
        }
    }
}
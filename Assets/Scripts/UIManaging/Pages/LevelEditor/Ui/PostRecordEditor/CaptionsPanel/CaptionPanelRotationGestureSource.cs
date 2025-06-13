using System;
using DigitalRubyShared;
using JetBrains.Annotations;
using Modules.InputHandling;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    [UsedImplicitly]
    internal sealed class CaptionPanelRotationGestureSource: IRotationGestureSource
    {
        public event Action<Vector2> RotationBegan;
        public event Action<float> RotationExecuted;
        public event Action RotationEnded;

        private readonly RotateGestureRecognizer _rotateGestureRecognizer;
        private readonly FingersScript _fingersScript;
        private bool _isEnabled;

        public CaptionPanelRotationGestureSource(FingersScript fingersScript)
        {
            _fingersScript = fingersScript;
            _rotateGestureRecognizer = new RotateGestureRecognizer();
            _rotateGestureRecognizer.AllowSimultaneousExecutionWithAllGestures();
            _rotateGestureRecognizer.StateUpdated += OnStateChanged;
        }

        public void SetViewPort(RectTransform viewPort)
        {
            _rotateGestureRecognizer.PlatformSpecificView = viewPort;
        }
        
        public void Activate()
        {
            if (_isEnabled) return;
            _isEnabled = true;
            _fingersScript.AddGesture(_rotateGestureRecognizer);
        }

        public void Deactivate()
        {
            if (!_isEnabled) return;
            _isEnabled = false;
            _fingersScript.RemoveGesture(_rotateGestureRecognizer);
        }
        
        private void OnStateChanged(GestureRecognizer gesture)
        {
            switch (gesture.State)
            {
                case GestureRecognizerState.Began:
                    var focus = new Vector2(gesture.FocusX, gesture.FocusY);
                    RotationBegan?.Invoke(focus);
                    break;
                case GestureRecognizerState.Executing:
                    RotationExecuted?.Invoke((gesture as RotateGestureRecognizer).RotationDegreesDelta);
                    break;
                case GestureRecognizerState.Ended:
                    RotationEnded?.Invoke();
                    break;
            }
        }
    }
}
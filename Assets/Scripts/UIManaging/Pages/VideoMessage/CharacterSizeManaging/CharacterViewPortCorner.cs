using System;
using System.Linq;
using DigitalRubyShared;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VideoMessage.CharacterSizeManaging
{
    internal sealed class CharacterViewPortCorner : MonoBehaviour
    {
        [Inject] private IVideoMessagePageGesturesControl _gesturesControl;
        [SerializeField] private CornerType _cornerType;

        private RectTransform _rectTransform;
        private bool _draggingOverViewPort;
        
        public event Action<ViewPortDragData> Dragging;

        public CornerType CornerType => _cornerType;

        private void Awake()
        {
            _gesturesControl.PanGestureStateChanged += OnPanGesture;
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            _gesturesControl.PanGestureStateChanged -= OnPanGesture;
        }

        private void OnPanGesture(PanGestureRecognizer panGesture)
        {
            if (panGesture.State != GestureRecognizerState.Began && panGesture.State != GestureRecognizerState.Executing && panGesture.State != GestureRecognizerState.Ended) return;
            if (panGesture.CurrentTrackedTouches.Count > 1) return;
            var touch = panGesture.CurrentTrackedTouches.First();
            if (panGesture.State == GestureRecognizerState.Began)
            {
                _draggingOverViewPort = IsTouchOverCornerElement(touch);
            }
            
            if (!_draggingOverViewPort) return;
            
            var data = new ViewPortDragData
            {
                Corner = this,
                CurrentPosition = new Vector2(touch.X, touch.Y),
                State = panGesture.State
            };
            Dragging?.Invoke(data);
        }

        private bool IsTouchOverCornerElement(GestureTouch touch)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, new Vector2(touch.X, touch.Y), null);
        }
    }

    internal struct ViewPortDragData
    {
        public CharacterViewPortCorner Corner;
        public Vector2 CurrentPosition;
        public GestureRecognizerState State;
    }

    internal enum CornerType
    {
        UpperLeft,
        UpperRight,
        BottomLeft,
        BottomRight
    }
}
using System;
using System.Threading.Tasks;
using Extensions;
using Sirenix.OdinInspector;
using UIManaging.Animated.Behaviours;
using UnityEngine;
using Zenject;

namespace UIManaging.Animated
{
    public sealed class KeyboardAnchoredUiAnimator : MonoBehaviour
    {
        [SerializeField] private int _keyboardHeightOffset = -70;
        [SerializeField] private Transform _target;
        [SerializeField] private SlideInOutBehaviour _slideBehaviour;

        [Inject] private INativeKeyboardHeightProvider _keyboardHeightProvider;
        private RectTransform _rootCanvas;
        private bool _initialized;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public State CurrentState { get; private set; }
        public int KeyboardHeightOffset
        {
            get => _keyboardHeightOffset;
            set => _keyboardHeightOffset = value;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _rootCanvas = transform.root.GetComponent<RectTransform>();
            _keyboardHeightProvider.Updated += OnKeyboardHeightUpdated;
        }

        private void OnDestroy()
        {
            _keyboardHeightProvider.Updated -= OnKeyboardHeightUpdated;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        [Button]
        public async void SlideUpInputField()
        {
            if (CurrentState == State.SlidingUp || CurrentState == State.Up) return;

            if (!_initialized)
            {
                await Initialize();
            }
            
            CurrentState = State.SlidingUp;

            _slideBehaviour.SlideIn(() => CurrentState = State.Up);
        }

        private async Task Initialize()
        {
            if (!_keyboardHeightProvider.HasData)
            {
                await WaitUntilKeyboardHeightIsAvailable();
            }
            
            var actualKeyboardHeight = ConvertToRectTransformLocalSize(_keyboardHeightProvider.TextStateKeyboardHeight);

            var inPosition = new Vector3(0, actualKeyboardHeight);
            var outPosition = Vector3.zero;
            _slideBehaviour.InitSequence(inPosition, outPosition);
            _initialized = true;
        }

        [Button]
        public void SlideDownInputField(Action onComplete = null)
        {
            if (CurrentState == State.SlidingDown || CurrentState == State.Down) return;
            
            CurrentState = State.SlidingDown;

            _slideBehaviour.SlideOut(() =>
            {
                CurrentState = State.Down;
                onComplete?.Invoke();
            });
        }

        [Button]
        public void MoveInputFieldInstantly(float height)
        {
            ((RectTransform)_target).SetAnchoredY(height);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private float GetCanvasHeightScaleFactor()
        {
            return _rootCanvas.GetHeight() / Screen.height;
        }

        private async Task WaitUntilKeyboardHeightIsAvailable()
        {
            while (!_keyboardHeightProvider.HasData)
            {
                await Task.Delay(15);
            }
        }
        
        private int ConvertToRectTransformLocalSize(int pixelsHeight)
        {
            var scaleFactor = GetCanvasHeightScaleFactor();
            return (int) (pixelsHeight * scaleFactor) + _keyboardHeightOffset;
        }
        
        private void OnKeyboardHeightUpdated(int height)
        {
            if (height == 0) return;
            
            MoveInputFieldInstantly(ConvertToRectTransformLocalSize(_keyboardHeightProvider.CurrentHeight));
        }
        
        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum State
        {
            Down,
            SlidingUp,
            Up,
            SlidingDown
        }
    }
}
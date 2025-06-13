using System;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.SearchPanel
{
    public class SearchPanelView : MonoBehaviour
    {
        private const float INPUT_COMPLETE_DELAY = 0.25f;
        private const int CHARACTER_LIMIT = 30;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action InputCleared;
        public event Action<string> InputCompleted;
        public event Action<string> InputOnSubmit;
        public event Action<string> ClearInputButtonClicked;
        public event Action<bool> KeyboardVisibilityChanged;
        
        protected event Action<bool> ControlsVisibilityChanged;

        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------
        
        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _searchInput;
        [SerializeField] private RectTransform _ingoredArea;
        [SerializeField] protected Button _clearInputButton;
        
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;

        private IInputFieldAdapter _inputFieldAdapter;
        private float _lastInputTime;
        private bool _hasNewInput;
        private string _previousInput = "";

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public UnityEvent<string> OnSelect => _searchInput.onSelect;
        public UnityEvent<string> OnDeselect => _searchInput.onDeselect;
        public bool HasInput => !string.IsNullOrEmpty(_inputFieldAdapter.Text);

        public string Text
        {
            get => InputFieldAdapter.Text;
            set => InputFieldAdapter.Text = value;
        }

        public string PlaceholderText
        {
            get => InputFieldAdapter.PlaceholderText;
            set => InputFieldAdapter.PlaceholderText = value;
        }

        private IInputFieldAdapter InputFieldAdapter =>
            _inputFieldAdapter ??
            (_inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_searchInput));

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _clearInputButton.onClick.AddListener(Clear);
            
            InputFieldAdapter.OnValueChanged += OnInputChanged;
            InputFieldAdapter.OnSubmit += OnSubmit;
            InputFieldAdapter.CharacterLimit = CHARACTER_LIMIT;

            InputFieldAdapter.OnKeyboardStatusChanged += OnKeyboardStatusChanged;

            #if UNITY_ANDROID
            if (_ingoredArea != null)
            {
                _searchInput.AddIgnoreDeselectOnRect(_ingoredArea);
            }
            #endif
        }

        private void OnKeyboardStatusChanged(KeyboardStatus status)
        {
            var isVisible = status == KeyboardStatus.Visible;
            
            KeyboardVisibilityChanged?.Invoke(isVisible);
        }

        private void Update()
        {
            if (!_hasNewInput || string.IsNullOrEmpty(_previousInput) || IsInputDelayNotExceeded())
            {
                return;
            }
            
            InputCompleted?.Invoke(_inputFieldAdapter.Text);
            _hasNewInput = false;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Select()
        {
            InputFieldAdapter.Select();
        }
        
        public void Deselect()
        {
            InputFieldAdapter.Deselect();
        }
        
        public void SetTextWithoutNotify(string text)
        {
            InputFieldAdapter.SetTextWithoutNotify(text);
            var controlsActive = !string.IsNullOrEmpty(text);
            _clearInputButton.gameObject.SetActive(controlsActive);
            ControlsVisibilityChanged?.Invoke(controlsActive);
        }
        
        public void ClearWithoutNotify()
        {
            InputFieldAdapter.SetTextWithoutNotify(string.Empty);
            _clearInputButton.gameObject.SetActive(false);
            ControlsVisibilityChanged?.Invoke(false);
        }
        
        public void Clear()
        {
            ClearInputButtonClicked?.Invoke(InputFieldAdapter.Text);
            InputFieldAdapter.Text = string.Empty;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnInputChanged(string input)
        {
            _hasNewInput = !input.Equals(_previousInput, StringComparison.OrdinalIgnoreCase);
            var controlsActive = !string.IsNullOrEmpty(input);
            _clearInputButton.gameObject.SetActive(controlsActive);
            ControlsVisibilityChanged?.Invoke(controlsActive);

            if (_hasNewInput)
            {
                _lastInputTime = Time.realtimeSinceStartup;
                _previousInput = input;
            }

            if (string.IsNullOrEmpty(input))
            {
                InputCleared?.Invoke();
            }
        }

        private void OnSubmit(string text)
        {
            InputOnSubmit?.Invoke(text);
        }

        private bool IsInputDelayNotExceeded()
        {
            return Time.realtimeSinceStartup - _lastInputTime < INPUT_COMPLETE_DELAY;
        }
    }
}
using System.Linq;
using Common;
using DigitalRubyShared;
using TMPro;
using UIManaging.Common.InputFields;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class PastePopup : BasePopup<PastePopupConfiguration>
    {
        private const float PADDING_BOTTOM = 50f;
        
        [SerializeField] private Button _button;
        
        [Inject] private FingersScript _fingersScript;

        private RectTransform _body;
        private TapGestureRecognizer _tapGesture = new TapGestureRecognizer { SendBeginState = true };
        private IInputFieldAdapter _inputField;
        private bool _isVisible;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        { 
            _body = (RectTransform)_button.transform;
        }

        private void OnDestroy()
        {
            ClearGesture();
        }

        protected override void OnConfigure(PastePopupConfiguration configuration)
        {
            _inputField = configuration.InputField;
            SetPassThroughObjects();
            ClearGesture();
            AddGesture();
            SetupPosition();
            Hide();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Show()
        {
            base.Show();
            _inputField.OnValueChanged += OnValueChanged;
            _button.onClick.AddListener(Paste);
            _isVisible = true;
        }

        public override void Hide()
        {
            base.Hide();
            _button.onClick.RemoveListener(Paste);
            _inputField.OnValueChanged -= OnValueChanged;
            _isVisible = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupPosition()
        {
            var inputFieldTransform = _inputField.Transform;
            var inputFieldRect = (RectTransform)inputFieldTransform;
            _body.position = new Vector3(_body.position.x, inputFieldTransform.position.y + inputFieldRect.rect.height + PADDING_BOTTOM, 0);
        }
        
        private void Paste()
        {
            var textEditor = new TextEditor { multiline = true };
            textEditor.Paste();
            var text = textEditor.text;
            
            if (!ValidatePaste(text)) return;
            _inputField.Text = text;
        }

        private void OnValueChanged(string text)
        {
            Hide();
        }
        
        private void UpdateGesture(GestureRecognizer gesture)
        {
            if (_tapGesture.State == GestureRecognizerState.Failed)
            {
                if (_isVisible)
                {
                    Hide();
                    return;
                }
            }
            if (_tapGesture.State != GestureRecognizerState.Began) return;
            
            if (_isVisible)
            {
                Hide();
                return;
            }
            
            if(_inputField.IsFocused) Show();
        }

        private void SetPassThroughObjects()
        {
            _inputField.Transform.tag = Constants.Gestures.ALLOW_GESTURE_PASSTHROUGH_TAG;
            var children = _inputField.Transform.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var child in children)
            {
                child.tag = Constants.Gestures.ALLOW_GESTURE_PASSTHROUGH_TAG;
            }
        }

        private void AddGesture()
        {
            _tapGesture.PlatformSpecificView = _inputField;
            _tapGesture.StateUpdated += UpdateGesture;
            _fingersScript.AddGesture(_tapGesture);
        }
        
        private void ClearGesture()
        {
            _tapGesture.StateUpdated -= UpdateGesture;
            _fingersScript.RemoveGesture(_tapGesture);
        }

        private bool ValidatePaste(string text)
        {
            if (_inputField.ContentType == TMP_InputField.ContentType.IntegerNumber)
            {
                return text.All(char.IsDigit);
            }
            
            return true;
        }
    }
}

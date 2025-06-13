using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class InputPopup : InformationPopup<InputPopupConfiguration>
    {
        [SerializeField] private InputField _inputField;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private string _inputString = string.Empty;

        //---------------------------------------------------------------------
        // Mono
        //---------------------------------------------------------------------

        private void Awake()
        {
            _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);

            _inputField.onValueChanged.AddListener(OnInputValueChanged);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
            _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            
            _inputField.onValueChanged.RemoveListener(OnInputValueChanged);
        }

        //---------------------------------------------------------------------
        // BasePopup
        //---------------------------------------------------------------------

        protected override void OnConfigure(InputPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);
            
            if (_inputField.placeholder is Text placeholder)
            {
                placeholder.text = configuration.PlaceholderText;
            }

            _inputField.text = string.Empty;
        }

        //---------------------------------------------------------------------
        // UI Callbacks
        //---------------------------------------------------------------------

        private void OnConfirmButtonClicked()
        {
            Hide(_inputString);
        }

        private void OnCancelButtonClicked()
        {
            Hide();
        }

        private void OnInputValueChanged(string newValue)
        {
            _inputString = newValue;
        }
    }
}
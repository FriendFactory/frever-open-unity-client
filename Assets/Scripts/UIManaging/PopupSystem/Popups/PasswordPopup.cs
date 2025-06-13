using System;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class PasswordPopup : BasePopup<PasswordPopupConfiguration>
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _confirmButton;

        private Action _onSuccess;
        private string _password;

        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        
        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        protected override void OnConfigure(PasswordPopupConfiguration configuration)
        {
            _inputField.text = string.Empty;
            _onSuccess = configuration.OnSuccess;
            _password = configuration.Password;
        }

        private void OnConfirmClicked()
        {
            Hide();
            
            if (_inputField.text == _password)
            {
                _onSuccess?.Invoke();
            }
        }
    }
}

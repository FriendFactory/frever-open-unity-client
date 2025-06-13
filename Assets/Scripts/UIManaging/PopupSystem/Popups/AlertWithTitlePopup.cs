using System;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal class AlertWithTitlePopup : DescriptionPopup<AlertPopupConfiguration>
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;

        private Action _onConfirm;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button.onClick.AddListener(OnConfirmButtonClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnConfirmButtonClicked);
        }


        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(AlertPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);
            _onConfirm = configuration.OnConfirm;
            _buttonText.text = configuration.ConfirmButtonText;
        }
        
        protected void OnConfirmButtonClicked()
        {
            _onConfirm?.Invoke();
            Hide();
        }
    }
}
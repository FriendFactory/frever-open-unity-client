using System;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class ServerDownPopup : InformationPopup<ServerDownPopupConfiguration>
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;

        private Action _onButtonClick;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(ServerDownPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);

            _onButtonClick = configuration.OnConfirm;
            _buttonText.text = configuration.ConfirmButtonText;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnButtonClicked()
        {
            _onButtonClick?.Invoke();
            Hide();
        }
    }
}

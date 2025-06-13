using System;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public class AlertPopup : InformationPopup<AlertPopupConfiguration>
    {
        [SerializeField] private TMP_Text _title;
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
            if (_title) _title.text = configuration.Title;

            _onConfirm = configuration.OnConfirm;
            if (_buttonText) _buttonText.text = configuration.ConfirmButtonText;
        }

        private void OnConfirmButtonClicked()
        {
            _onConfirm?.Invoke();
            Hide();
        }
    }
}

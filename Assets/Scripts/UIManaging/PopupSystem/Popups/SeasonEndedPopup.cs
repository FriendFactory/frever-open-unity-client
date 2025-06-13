using System;
using Common;
using Extensions;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.LevelCreation;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class SeasonEndedPopup: InformationPopup<SeasonEndedPopupConfiguration>
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private ImageByKeyLoader _imageByKeyLoader;
        [SerializeField] private Image _image;
        
        private Action _onClick;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
            _image.SetAlpha(0);
            _imageByKeyLoader.Loaded += () => { _image.SetAlpha(1); };
            _imageByKeyLoader.LoadImageAsync(Constants.FileKeys.CREATE_FREVER_CHARACTER_POPUP);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(SeasonEndedPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);
            
            _onClick = configuration.OnButtonClick;

            if (_buttonText != null && configuration.ButtonText != null)
            {
                _buttonText.text = configuration.ButtonText;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnButtonClicked()
        {
            _onClick?.Invoke();
        }
    }
}
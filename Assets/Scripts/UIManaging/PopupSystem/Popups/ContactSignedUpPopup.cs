using System;
using Navigation.Core;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class ContactSignedUpPopup : InformationPopup<ContactSignedPopupConfiguration>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _checkItOutButton;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Image _popupImage;
        [SerializeField] private Image _popupBackgroundImage;
        [SerializeField] private Sprite[] _popupSprites;
        [SerializeField] private string[] _hexColors;

        private Action _onCheckItOut;

        [Inject] private PageManager _pageManager;
        
        //---------------------------------------------------------------------
        // Mono
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _checkItOutButton.onClick.AddListener(OnCheckItOutButtonClicked);
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDestroy()
        {
            _checkItOutButton.onClick.RemoveListener(OnCheckItOutButtonClicked);
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
        
        //---------------------------------------------------------------------
        // BasePopup
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(ContactSignedPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);
            
            _onCheckItOut = configuration.OnCheckItOut;
            _title.text = configuration.TextMeshTitle;
            SetupPopupImage();
        }
        
        //---------------------------------------------------------------------
        // Button Callbacks
        //---------------------------------------------------------------------

        private void OnCheckItOutButtonClicked()
        {
            _onCheckItOut?.Invoke();
        }
        
        private void OnCloseButtonClicked()
        {
            Hide();
        }
        
        //---------------------------------------------------------------------
        // Other
        //---------------------------------------------------------------------

        private void SetupPopupImage()
        {
            var randIndex = Random.Range(0, _popupSprites.Length);
            var sprite = _popupSprites[randIndex];
            _popupImage.sprite = sprite;
            _popupImage.SetNativeSize();

            if (ColorUtility.TryParseHtmlString(_hexColors[randIndex], out var color))
            {
                _popupBackgroundImage.color = color;
            } 
        }
    }
}
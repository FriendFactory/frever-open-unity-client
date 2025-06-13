using System;
using Common.Publishers;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class PublishTypeSelectionButton: MonoBehaviour
    {
        [SerializeField] private PublishingType _publishingType;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _iconIsOn;
        [SerializeField] private Color _iconOnColor = Color.white;
        [SerializeField] private Sprite _iconIsOff;
        [SerializeField] private Color _iconOffColor = Color.white;
        [SerializeField] private Image _background;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public PublishingType PublishingType => _publishingType;
        public bool IsSelected => _toggle.isOn;
        
        public event Action<PublishingType> Selected;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool isOn)
        {
            RefreshView();
            if (isOn)
            {
                Selected?.Invoke(_publishingType);
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(bool isSelected)
        {
            _toggle.isOn = isSelected;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RefreshView()
        {
            _icon.sprite = _toggle.isOn ? _iconIsOn : _iconIsOff;
            _icon.color = _toggle.isOn ? _iconOnColor : _iconOffColor;
            _background.SetActive(_toggle.isOn);
        }
    }
}
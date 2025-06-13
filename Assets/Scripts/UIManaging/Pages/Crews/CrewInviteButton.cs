using System;
using Abstract;
using TMPro;
using UIManaging.Localization;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    [RequireComponent(typeof(Button))]
    internal sealed class CrewInviteButton : BaseContextDataView<bool>
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _buttonLabel;
        [SerializeField] private Image _buttonIcon;
        [SerializeField] private Image _buttonBackground;

        [Space] 
        [SerializeField] private Color _activeTextColor;
        [SerializeField] private Sprite _activeIcon;
        [SerializeField] private Sprite _activeBackground;
        
        [SerializeField] private Color _disabledTextColor;
        [SerializeField] private Sprite _disabledIcon;
        [SerializeField] private Sprite _disabledBackground;

        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private CrewPageLocalization _localization;

        public Action OnInviteActionRequested;

        public bool Interactable
        {
            set => _button.interactable = value;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            if (!ContextData)
            {
                ApplyActiveStyle();
                return;
            }
            ApplyDisabledStyle();
        }

        private void ApplyActiveStyle()
        {
            if (_buttonIcon != null) _buttonIcon.sprite = _activeIcon;
            _buttonLabel.text = _localization.UserItemInviteButton;
            _buttonLabel.color = _activeTextColor;
            _buttonBackground.sprite = _activeBackground;
        }

        private void ApplyDisabledStyle()
        {
            if (_buttonIcon != null) _buttonIcon.sprite = _disabledIcon;
            _buttonLabel.text = _localization.UserItemInviteSentButton;
            _buttonLabel.color = _disabledTextColor;
            _buttonBackground.sprite = _disabledBackground;
        }

        private void OnClicked()
        {
            OnInviteActionRequested?.Invoke();
        }
    }
}
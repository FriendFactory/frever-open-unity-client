using System;
using TMPro;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class InviterFollowedSnackbar : SnackBar<InviterFollowedSnackbarConfiguration>
    {
        [SerializeField] private TMP_Text _message;
        [SerializeField] private Button _button;

        private Action _onClick;
        
        public override SnackBarType Type => SnackBarType.InviterFollowed;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
        }
        
        protected override void OnConfigure(InviterFollowedSnackbarConfiguration configuration)
        {
            _message.text = $"@{configuration.Nickname}";
            _onClick = configuration.OnClick;
            _button.interactable = true;
        }

        private void OnButtonClick()
        {
            _button.interactable = false;
            _onClick?.Invoke();
        }
    }
}
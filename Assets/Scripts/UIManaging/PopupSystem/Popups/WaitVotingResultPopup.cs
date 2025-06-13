using System;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class WaitVotingResultPopup: BasePopup<WaitVotingResultPopupConfiguration>
    {
        [SerializeField] private Button _okButton;

        private void Awake()
        {
            _okButton.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(WaitVotingResultPopupConfiguration configuration)
        {
        }
    }
}
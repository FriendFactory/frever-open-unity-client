using System;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class ExitVotingPopup: BasePopup<ExitVotingPopupConfiguration>
    {
        [SerializeField] private Button _confirmExitButton;
        [SerializeField] private Button _cancelButton;

        private void Awake()
        {
            _confirmExitButton.onClick.AddListener(() =>
            {
                Hide();
                Configs.ExitConfirmed?.Invoke();
            });
            _cancelButton.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(ExitVotingPopupConfiguration configuration)
        {
            
        }
    }
}
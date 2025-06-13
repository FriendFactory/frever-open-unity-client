using System;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public sealed class CopyrightFailedPopupConfiguration : PopupConfiguration
    {
        public CopyrightFailedPopupConfiguration()
        {
            PopupType = PopupType.CopyrightFailedPopup;
        }
    }

    internal sealed class CopyrightFailedPopup: BasePopup<CopyrightFailedPopupConfiguration>
    {
        [SerializeField] private Button _close;

        private void Awake()
        {
            _close.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(CopyrightFailedPopupConfiguration configuration)
        {
            
        }
    }
}
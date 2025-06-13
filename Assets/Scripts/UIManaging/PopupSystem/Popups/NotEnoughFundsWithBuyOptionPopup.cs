using Extensions;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class NotEnoughFundsWithBuyOptionPopup : BasePopup<NotEnoughFundsWithBuyOptionPopupConfiguration>
    {
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _cancel;

        private void Awake()
        {
            _buyButton.onClick.AddListener(()=> Configs.OnBuyClicked?.Invoke());
            _cancel.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(NotEnoughFundsWithBuyOptionPopupConfiguration configuration)
        {
        }
    }
}
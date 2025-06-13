using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class RevokeAppleTokenInstructionPopup: BasePopup<RevokeAppleTokenPopupConfiguration>
    {
        [SerializeField] private Button _openSettingsButton;

        private void Awake()
        {
            _openSettingsButton.onClick.AddListener(OnOpenSettingsButtonClicked);
        }

        protected override void OnConfigure(RevokeAppleTokenPopupConfiguration configuration)
        {

        }

        private void OnOpenSettingsButtonClicked()
        {
            Configs.OnOpenSettingsClicked?.Invoke();
        }
    }
}
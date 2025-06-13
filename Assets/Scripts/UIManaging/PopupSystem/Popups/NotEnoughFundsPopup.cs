using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class NotEnoughFundsPopup : BasePopup<NotEnoughFundsPopupConfiguration>
    {
        private const string ONBOARDING_TEXT = "Buy or earn more currency after onboarding";
        private const string DEFAULT_TEXT = "You need to buy or earn more gems";
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Button _okButton;

        private void Awake()
        {
            _okButton.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(NotEnoughFundsPopupConfiguration configuration)
        {
            _description.text = configuration.IsOnboarding ? ONBOARDING_TEXT : DEFAULT_TEXT;

        }
    }
}

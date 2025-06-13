using Extensions;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Popups.Store.SeasonPassProposal.Popup
{
    internal sealed class PremiumPassPurchasedPopup: BasePopup<PremiumPassPurchasedPopupConfiguration>
    {
        [SerializeField] private Button _seasonRewardsButton;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _seasonRewardsButton.onClick.AddListener(() =>
            {
                Configs.OnSeasonRewardsButtonClicked?.Invoke();
            });
            
            _closeButton.onClick.AddListener(() =>
            {
                Configs.OnExitClicked?.Invoke();
                Hide();
            });
        }

        protected override void OnConfigure(PremiumPassPurchasedPopupConfiguration configuration)
        {
            _seasonRewardsButton.SetActive(configuration.ShowSeasonRewardsButton);
        }
    }
}
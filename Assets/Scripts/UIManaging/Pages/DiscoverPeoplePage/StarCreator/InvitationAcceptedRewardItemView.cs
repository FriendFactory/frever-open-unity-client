using System.Threading;
using Abstract;
using Bridge;
using TMPro;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.Rewards.Invitation.Popups;
using UIManaging.Rewards.Models;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal sealed class InvitationAcceptedRewardItemView: BaseContextDataView<InvitationAcceptedRewardModel>
    {
        [SerializeField] private TMP_Text _message;
        [SerializeField] private TMP_Text _softCurrencyAmount;
        [SerializeField] private Button _claimButton;

        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackbarHeelper;
        [Inject] private ProfileLocalization _localization;

        private CancellationTokenSource _tokenSource;
        
        protected override void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();
            
            UpdateTextFields();
            
            _claimButton.onClick.AddListener(ClaimReward);
        }

        protected override void BeforeCleanup()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            
            _claimButton.onClick.RemoveListener(ClaimReward);
        }

        private void UpdateTextFields()
        {
            var softCurrency = ContextData.SoftCurrency.ToString();
            var message = string.Format(_localization.InvitedUserSignedUpTitleFormat, ContextData.NickName);
            
            _softCurrencyAmount.SetText(softCurrency);
            _message.SetText(message);
        }

        private async void ClaimReward()
        {
            var result = await _bridge.GetProfile(ContextData.Id, _tokenSource.Token);
            if(result.IsRequestCanceled) return;

            if (result.IsError || result.Profile is null)
            {
                var claimResult = await _bridge.ClaimRewardForInvitedUser();
                if (claimResult.IsError)
                {
                    Debug.LogError(claimResult.ErrorMessage);
                    return;
                }
                if(claimResult.IsRequestCanceled) return;
                
                _snackbarHeelper.ShowSuccessDarkSnackBar($"<b>@{ContextData.NickName}</b> does not exist, unable to add to friends list.");
                ContextData.ClaimReward();
                
                return;
            }
            
            var configuration = new InvitationAcceptedRewardPopupConfiguration(ContextData, result.Profile);
            
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.InvitationAcceptedReward);
        }
    }
}
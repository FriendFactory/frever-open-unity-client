using System;
using System.Threading.Tasks;
using Bridge;
using Common;
using Common.Abstract;
using Common.UserBalance;
using Extensions;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    internal sealed class UmaEditorStartingGiftController: BaseContextPanel<UmaEditorArgs>
    {
        [SerializeField] private RewardFlowManager _rewardFlowManager;
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private GameObject _greyOverlay;
        [SerializeField] private GiftOverlay _giftOverlay;
        
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private UmaLevelEditorPanelModel _umaLevelEditorPanelModel;
        [Inject] private IBridge _bridge;
        [Inject] private IMetadataProvider _metadataProvider;
        
        private bool IsOnboarding => ContextData.ConfirmActionType == CharacterEditorConfirmActionType.Onboarding;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool HasStartGift => !IsOnboarding && (!_localUserDataHolder.InitialAccountBalance?.IsAdded ?? false) 
                                                   && _metadataProvider.MetadataStartPack.GetUniverseByGenderId(ContextData.Gender.Id).AllowStartGift;
        
        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _umaLevelEditorPanelModel.PanelOpened += OnPanelOpened;
        }

        protected override void BeforeCleanUp()
        {
            _umaLevelEditorPanelModel.PanelOpened -= OnPanelOpened;
        }

        private async void OnPanelOpened()
        {
            try
            {
                _umaLevelEditorPanelModel.PanelOpened -= OnPanelOpened;

                if (HasStartGift)
                {
                    await HandleStartGiftAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task HandleStartGiftAsync()
        {
            var completionSource = new TaskCompletionSource<bool>();
            
            var userBalanceSettings = new UserBalanceArgs(_localUserDataHolder.UserBalance?.SoftCurrencyAmount ?? 0, _localUserDataHolder.UserBalance?.HardCurrencyAmount ?? 0);
            var userBalanceModel = new AnimatedUserBalanceModel(userBalanceSettings);
            
            _userBalanceView.Initialize(userBalanceModel);
            
            if (_localUserDataHolder.UserBalance is null) await _localUserDataHolder.UpdateBalance();
            
            var userBalance = _localUserDataHolder.UserBalance;
            _rewardFlowManager.Initialize(userBalance.SoftCurrencyAmount, userBalance.HardCurrencyAmount, 1, 0);

            var softAmount = _localUserDataHolder.InitialAccountBalance.SoftCurrency; 
            var hardAmount = _localUserDataHolder.InitialAccountBalance.HardCurrency;
            
            _greyOverlay.SetActive(true);
            _giftOverlay.Show(softAmount, hardAmount, OnFlowCompleted);
            
            await completionSource.Task;
            
            async void OnFlowCompleted()
            {
                await ClaimWelcomeGiftAsync(completionSource);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private async Task ClaimWelcomeGiftAsync(TaskCompletionSource<bool> completionSource)
        {
            var result = await _bridge.ClaimWelcomeGift();

            if (result.IsError)
            {
                Debug.LogError($"Failed to claim welcome gift, reason: {result.ErrorMessage}");
                _greyOverlay.SetActive(false);
                
                completionSource.SetResult(true);
                return;
            }

            var newSoftAmount = _localUserDataHolder.UserBalance.SoftCurrencyAmount + _localUserDataHolder.InitialAccountBalance.SoftCurrency;
            var newHardAmount = _localUserDataHolder.UserBalance.HardCurrencyAmount + _localUserDataHolder.InitialAccountBalance.HardCurrency;
            _localUserDataHolder.UserBalance.SoftCurrencyAmount = newSoftAmount;
            _localUserDataHolder.UserBalance.HardCurrencyAmount = newHardAmount;

            _localUserDataHolder.InitialAccountBalance.IsAdded = true;
            PlayerPrefs.SetInt(Constants.Onboarding.RECEIVED_START_GIFT_IDENTIFIER, 1);
            _rewardFlowManager.StartAnimation(0, newSoftAmount, newHardAmount);
            _rewardFlowManager.FlowCompleted += OnGiftFlowCompleted;

            void OnGiftFlowCompleted(RewardFlowResult rewardFlowResult)
            {
                _rewardFlowManager.FlowCompleted -= OnGiftFlowCompleted;
                _greyOverlay.SetActive(false);
                
                completionSource.SetResult(true);
            }
        }
    }
}
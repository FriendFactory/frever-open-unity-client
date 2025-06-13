using Bridge.Models.ClientServer.Tasks;
using Extensions;
using Modules.Sound;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks.RewardPopUp
{
    public class TaskCompletedPopup : BasePopup<TaskCompletedPopupConfiguration>
    {
        [Inject] private ISoundManager _soundManager;

        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _claimButton;
        
        [Space(10)]
        [SerializeField] private TaskCompletedPopupRewardElement _softCurrencyReward;
        [SerializeField] private TaskCompletedPopupRewardElement _hardCurrencyReward;
        [SerializeField] private TaskCompletedPopupRewardElement _experienceReward;

        private int _softCurrencyPayout;
        private int _hardCurrencyPayout;
        private int _experiencePayout;
        private Coroutine _playRewardSoundsRoutine;
        private ClaimPopupSoundsTrigger _soundsTrigger;

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
            _claimButton.onClick.RemoveListener(OnCloseButtonClick);

            if (_playRewardSoundsRoutine != null) StopCoroutine(_playRewardSoundsRoutine);
        }

        protected override void OnConfigure(TaskCompletedPopupConfiguration configuration)
        {
            _softCurrencyReward.SetActive(false);
            _hardCurrencyReward.SetActive(false);
            _experienceReward.SetActive(false);

            _softCurrencyPayout = configuration.SoftCurrencyPayout;
            _hardCurrencyPayout = configuration.HardCurrencyPayout;
            _experiencePayout = configuration.ExperiencePayout;
            
            _claimButton.onClick.AddListener(OnCloseButtonClick);
            _closeButton.onClick.AddListener(OnCloseButtonClick);
            
            SetupRewardDetails();
        }

        private void SetupRewardDetails()
        {
            if (_softCurrencyPayout > 0)
            {
                ShowRewardDetail(_softCurrencyReward, _softCurrencyPayout);
            }

            if (_hardCurrencyPayout != 0)
            {
                ShowRewardDetail(_hardCurrencyReward, _hardCurrencyPayout);
            }

            if (_experiencePayout != 0)
            {
                ShowRewardDetail(_experienceReward, _experiencePayout);
            }
        }

        private void ShowRewardDetail(TaskCompletedPopupRewardElement element, long amount)
        {
            element.SetActive(true);
            element.Show(amount.ToString());
        }
        
        private void OnCloseButtonClick()
        {
            gameObject.SetActive(false);
            Hide();
        }

        private void OnSoundsFinished()
        {
            _playRewardSoundsRoutine = null;
        }
    }
}
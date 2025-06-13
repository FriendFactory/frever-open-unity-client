using System;
using Extensions;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Tasks.RewardPopUp;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks.TaskRewardFlow
{
    public sealed class GiftOverlay : MonoBehaviour
    {
        [SerializeField] private Button _claimButton;
        [SerializeField] private TMP_Text _description;

        [Header("Reward Section")] 
        [SerializeField] private TaskCompletedPopupRewardElement _softCurrencyRewardElement;
        [SerializeField] private TaskCompletedPopupRewardElement _hardCurrencyRewardContainer;

        [Inject] private CharacterEditorLocalization _localization;
        
        private event Action _onClose;

        public void Show(int softCurrencyAmount, int hardCurrencyAmount, Action onClaimButtonClick)
        {
            _onClose += onClaimButtonClick;
            gameObject.SetActive(true);
            SetupRewardSection(softCurrencyAmount, hardCurrencyAmount);
            SetupButtonCallbacks();
            _description.text = string.Format(_localization.WelcomeGiftOverlayDescriptionFormat, softCurrencyAmount);
        }

        private void SetupButtonCallbacks()
        {
            _claimButton.onClick.AddListener(Close);
        }

        private void Close()
        {
            _onClose?.Invoke();
            _onClose = null;
            gameObject.SetActive(false);
        }

        private void SetupRewardSection(int softCurrencyAmount, int hardCurrencyAmount)
        {
            _softCurrencyRewardElement.SetActive(softCurrencyAmount > 0);
            _softCurrencyRewardElement.Show(softCurrencyAmount.ToString());
            
            _hardCurrencyRewardContainer.SetActive(hardCurrencyAmount >0);
            _hardCurrencyRewardContainer.Show(hardCurrencyAmount.ToString());
        }
    }
}
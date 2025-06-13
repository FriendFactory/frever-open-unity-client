using System;
using Extensions;
using Modules.Amplitude;
using Modules.QuestManaging;
using TMPro;
using UIManaging.Core;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Common
{
    public class QuestButton: ButtonBase
    {
        private readonly int _completeTriggerHash = Animator.StringToHash("Complete");
        
        [SerializeField] private TextMeshProUGUI _questCounter;
        [SerializeField] private GameObject _questReadyIcon;
        [SerializeField] private Animator _questCompleteAnimator;
        
        [Inject] private PopupManager _popupManager;
        [Inject] private IQuestManager _questManager;
        [Inject] private AmplitudeManager _amplitudeManager;

        public event Action HidingBegin;

        public void QuestCompleteAnimation()
        {
            _questCompleteAnimator.SetTrigger(_completeTriggerHash);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (!_amplitudeManager.IsOnboardingQuestsFeatureEnabled() || _questManager.IsComplete)
            {
                gameObject.SetActive(false);
                return;
            }

            Interactable = true;
            _questManager.QuestDataUpdated += UpdateCounter;
            UpdateCounter();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (_questManager.IsComplete)
            {
                return;
            }
            
            _questManager.QuestDataUpdated -= UpdateCounter;
        }
        
        protected override void OnClick()
        {
            var config = new QuestPopupConfiguration(OnHidingBegin);
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnHidingBegin()
        {
            HidingBegin?.Invoke();
        }

        private void UpdateCounter()
        {
            var questRemaining = _questManager.QuestsRemainingInCurrentGroup;
            
            _questCounter.text = questRemaining.ToString();
            _questCounter.SetActive(questRemaining > 0);
            _questReadyIcon.SetActive(questRemaining == 0);
        }
    }
}
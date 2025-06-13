using System;
using System.Collections;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class DirectMessagesLockedPopup: ConfigurableBasePopup<DirectMessagesLockedPopupConfiguration>
    {
        private const float UPDATE_PERIOD = 5f;
        
        [SerializeField] private TMP_Text _hoursText;
        [SerializeField] private TMP_Text _minutesText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _overlayButton;

        [Inject] private LocalUserDataHolder _localUser;
        
        private Coroutine _timerCoroutine;
        
        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(Hide);
            _closeButton.onClick.AddListener(Hide);
            _overlayButton.onClick.AddListener(Hide);

            if (_localUser != null)
            {
                _timerCoroutine = StartCoroutine(TimerCoroutine());
            }
        }

        private void Start()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
            
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private IEnumerator TimerCoroutine()
        {
            do
            {
                var timeLeft = _localUser.UserProfile.ChatAvailableAfterTime - DateTime.UtcNow;

                _hoursText.text = Mathf.Max((int)Math.Floor(timeLeft.TotalHours), 0).ToString();
                _minutesText.text = Mathf.Max(timeLeft.Minutes, 0).ToString();

                yield return new WaitForSeconds(UPDATE_PERIOD);
            } 
            while (_timerCoroutine != null);
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(Hide);
            _closeButton.onClick.RemoveListener(Hide);
            _overlayButton.onClick.RemoveListener(Hide);
            _timerCoroutine = null;
        }

        protected override void OnConfigure(DirectMessagesLockedPopupConfiguration configuration)
        {
            
        }
    }
}
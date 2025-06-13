using System;
using System.Collections;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal abstract class DialogPopup<TConfiguration> : InformationPopup<TConfiguration>
        where TConfiguration : DialogPopupConfiguration
    {
        [SerializeField] private Button _yesButton;
        [SerializeField] private TextMeshProUGUI _yesButtonText;
        [SerializeField] private Button _noButton;
        [SerializeField] private TextMeshProUGUI _noButtonText;
        [SerializeField] private Button _overlayButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private RectTransform _bodyRect;
        
        private Action _onYes;
        private Action _onNo;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _yesButton.onClick.AddListener(OnYesButtonClicked);
            if (_noButton) _noButton.onClick.AddListener(OnNoButtonClicked);
            if (_overlayButton) _overlayButton.onClick.AddListener(OnNoButtonClicked);
            if (_closeButton) _closeButton.onClick.AddListener(OnNoButtonClicked);
        }

        private void OnDestroy()
        {
            _yesButton.onClick.RemoveListener(OnYesButtonClicked);
            if (_noButton) _noButton.onClick.RemoveListener(OnNoButtonClicked);
            if (_overlayButton) _overlayButton.onClick.RemoveListener(OnNoButtonClicked);
            if (_closeButton) _closeButton.onClick.RemoveListener(OnNoButtonClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(TConfiguration configuration)
        {
            base.OnConfigure(configuration);
            
            _onYes = configuration.OnYes;
            _onNo = configuration.OnNo;

            if (_yesButtonText != null && configuration.YesButtonText != null)
            {
                _yesButtonText.text = configuration.YesButtonText;
            }

            if (_noButtonText != null && configuration.NoButtonText != null)
            {
                _noButtonText.text = configuration.NoButtonText;
            }
        }

        public override void Show()
        {
            base.Show();
            StartCoroutine(RebuildLayout());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnYesButtonClicked()
        {
            _onYes?.Invoke();
            Hide(true);
        }

        private void OnNoButtonClicked()
        {
            _onNo?.Invoke();
            Hide(false);
        }
        
        private IEnumerator RebuildLayout()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_bodyRect);
        }
    }

    internal sealed class DialogPopup : DialogPopup<DialogPopupConfiguration>
    {
    }
}
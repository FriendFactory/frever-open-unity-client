using System;
using System.Collections;
using Extensions;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal abstract class DialogDarkPopup<TConfiguration> : DescriptionPopup<TConfiguration>
        where TConfiguration : DialogDarkPopupConfiguration
    {
        [SerializeField] private Button _yesButton;
        [SerializeField] private TextMeshProUGUI _yesButtonText;
        [SerializeField] private Button _noButton;
        [SerializeField] private TextMeshProUGUI _noButtonText;
        [SerializeField] private Button _overlayButton;
        [SerializeField] private RectTransform _bodyRect;
        
        [SerializeField] private Color _textDefaultColor;
        [SerializeField] private Color _textRedColor;

        private Image _background;
        private Color _backgroundDefaultColor;
        private Action _onYes;
        private Action _onNo;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _yesButton.onClick.AddListener(OnYesButtonClicked);
            _noButton.onClick.AddListener(OnNoButtonClicked);
            _overlayButton?.onClick.AddListener(OnNoButtonClicked);
            _background = _bodyRect.GetComponent<Image>();
            _backgroundDefaultColor = _background.color;
        }

        private void OnDestroy()
        {
            _yesButton.onClick.RemoveListener(OnYesButtonClicked);
            _noButton.onClick.RemoveListener(OnNoButtonClicked);
            _overlayButton?.onClick.RemoveListener(OnNoButtonClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(TConfiguration configuration)
        {
            base.OnConfigure(configuration);
            _background.color = configuration.CustomBackgroundColor ?? _backgroundDefaultColor;
            _onYes = configuration.OnYes;
            _onNo = configuration.OnNo;

            if (_yesButtonText != null)
            {
                _yesButtonText.text = configuration.YesButtonText;
                _yesButtonText.color = configuration.YesButtonSetTextColorRed ? _textRedColor : _textDefaultColor;
            }

            _noButton.SetActive(!string.IsNullOrEmpty(configuration.NoButtonText));
            if (_noButtonText != null)
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

    internal sealed class DialogDarkPopup : DialogDarkPopup<DialogDarkPopupConfiguration>
    {
    }
}
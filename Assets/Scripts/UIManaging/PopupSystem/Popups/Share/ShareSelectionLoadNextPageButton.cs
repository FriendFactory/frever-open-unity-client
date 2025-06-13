using System;
using Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionLoadNextPageButton : BaseContextDataView<ShareSelectionNextPageButtonModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _canvasGroup;
        [Header("State dependant settings")]
        [SerializeField] private float _enabledAlpha = 1f;
        [SerializeField] private float _disabledAlpha = 0.2f;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        protected override void OnInitialized()
        {
            OnStateChanged(ContextData.State);
            
            ContextData.StateChanged += OnStateChanged;
        }

        protected override void BeforeCleanup()
        {
            ContextData.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(SearchNextButtonState state)
        {
            switch (state)
            {
                case SearchNextButtonState.Enabled:
                    Enable();
                    break;
                case SearchNextButtonState.Disabled:
                    Disable();
                    break;
                case SearchNextButtonState.Busy:
                    SetBusy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnClick()
        {
            ContextData?.Click();
        }

        private void Disable()
        {
            _canvasGroup.alpha = _disabledAlpha;
            _canvasGroup.interactable = false;
        }

        private void Enable()
        {
            _canvasGroup.alpha = _enabledAlpha;
            _canvasGroup.interactable = true;
        }

        private void SetBusy()
        {
            Disable();
        }
    }
}
using System;
using DG.Tweening;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public class ConfirmCoinPurchasePopup : BasePopup<ConfirmCoinPurchasePopupConfiguration>
    {
        private const string CURRENCY_ICON = "<sprite index=0>";
        private const float SLIDE_IN_DURATION = 0.3f;
        private const float SLIDE_OUT_DURATION = 0.3f;
        
        [SerializeField] private TextMeshProUGUI _softCurrencyAmountText;
        [SerializeField] private TextMeshProUGUI _hardCurrencyAmountText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _overlayButton;
        [SerializeField] private RectTransform _bodyRect;
        
        private Vector2 _slideInPosition;
        private Vector2 _startPosition;

        private Action _onConfirm;

        private void Awake()
        {
            Setup();
        }
        
        protected override void OnConfigure(ConfirmCoinPurchasePopupConfiguration configuration)
        {
            _softCurrencyAmountText.text = configuration.SoftCurrencyAmount;
            _hardCurrencyAmountText.text = $"{CURRENCY_ICON}{configuration.HardCurrencyCost}";
            _onConfirm = configuration.OnConfirm;
        }

        public override void Show()
        {
            base.Show();
            SlideIn();
        }

        private void Setup()
        {
            _confirmButton.onClick.AddListener(Confirm);
            _cancelButton.onClick.AddListener(SlideOut);
            _overlayButton.onClick.AddListener(SlideOut);
            
            var anchoredPosition = _bodyRect.anchoredPosition;
            _slideInPosition = new Vector3(anchoredPosition.x, anchoredPosition.y + _bodyRect.rect.height);
            _startPosition = anchoredPosition;
        }

        private void Confirm()
        {
            SlideOut();
            _onConfirm?.Invoke();
        }

        private void SlideOut()
        {
            _bodyRect.DOAnchorPos(_startPosition, SLIDE_OUT_DURATION).OnComplete(Hide);
        }
        
        private void SlideIn()
        {
            _bodyRect.DOAnchorPos(_slideInPosition, SLIDE_IN_DURATION).SetEase(Ease.OutQuad);
        }
    }
}

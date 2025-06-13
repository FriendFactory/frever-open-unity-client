using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Modules.InAppPurchasing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Popups.Store.SuccessWindow
{
    internal sealed class CurrencyPurchaseSuccessWindow : MonoBehaviour
    {
        private const float CLOSE_FADE_DURATION = 0.5f;
            
        [SerializeField] private Animator _animator;
        [SerializeField] private CanvasGroup _bodyCanvasGroup;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _bigIcon;
        [SerializeField] private Image _smallIcon;
        [SerializeField] private TextMeshProUGUI _currencyAmount;
        [SerializeField] private List<PurchaseSuccessWindowContent> _contents = new List<PurchaseSuccessWindowContent>();

        private Action _onClose;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Close);
            _confirmButton.onClick.AddListener(Close);
        }

        public void Show(string currencyAmount, ProductType currencyType, Action onClose)
        {
            _onClose = onClose;
            gameObject.SetActive(true);
            _animator.enabled = true;
            _currencyAmount.text = currencyAmount;
            SetupContent(currencyType);
        }

        private void SetupContent(ProductType type)
        {
            var targetContent = _contents.First(x => x.Type == type);
            _bigIcon.sprite = targetContent.BigCurrencyIcon;
            _bigIcon.SetNativeSize();
            _smallIcon.sprite = targetContent.SmallCurrencyIcon;
        }

        private void Close()
        {
            _animator.enabled = false;
            _bodyCanvasGroup.DOFade(0, CLOSE_FADE_DURATION).OnComplete(()=> gameObject.SetActive(false));
            _onClose?.Invoke();
        }
    }
}

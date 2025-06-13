using System;
using Extensions;
using TMPro;
using UIManaging.Pages.OnBoardingPage.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.AccountVerification
{
    internal sealed class LoadingButton: MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _glow;
        [SerializeField] private LoadingIndicator _loadingIndicator;
        
        private bool _isLoading;

        public event Action Clicked;

        public bool Interactable
        {
            get => _button.interactable;
            set
            {
                _button.interactable = value;
                _glow.SetActive(value);
            }
        }

        public string Label
        {
            get => _label.text;
            set => _label.text = value;
        }

        public void ToggleLoading(bool isOn)
        {
            _isLoading = isOn;
            
            _label.SetActive(!_isLoading);
            _loadingIndicator.SetActive(_isLoading);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (_isLoading) return;
            
            Clicked?.Invoke();
        }
    }
}
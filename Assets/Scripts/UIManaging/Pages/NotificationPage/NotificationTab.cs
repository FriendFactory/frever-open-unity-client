using System;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UIManaging.Pages.NotificationPage
{
    internal sealed class NotificationTab: MonoBehaviour
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private NotificationTabType _tabType;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _image;
        [SerializeField] private Color _backgroundColorOn = Color.white;
        [SerializeField] private Color _textColorOn = Color.black;
        [SerializeField] private Color _backgroundColorOff = Color.gray;
        [SerializeField] private Color _textColorOff = Color.white;
        
        public NotificationTabType Type => _tabType;

        public Action<NotificationTabType> Clicked;
        private bool _isSelected;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnClicked);
        }

        public void Init(bool isSelected)
        {
            _toggle.SetIsOnWithoutNotify(isSelected);
            RefreshState();
        }

        public void SetCount(int count)
        {
            _countText.text = count.ToString();
            _countText.transform.parent.SetActive(count > 0);
            _header.horizontalAlignment = count == 0 ? HorizontalAlignmentOptions.Center : HorizontalAlignmentOptions.Right;
        }
        
        private void OnClicked(bool isOn)
        {
            if (_isSelected && isOn) return;
            RefreshState();
            if (!isOn) return;
            Clicked?.Invoke(Type);
        }

        private void RefreshState()
        {
            _image.color = _toggle.isOn ? _backgroundColorOn : _backgroundColorOff;
            _header.color = _toggle.isOn ? _textColorOn : _textColorOff;
            _isSelected = _toggle.isOn;
        }
    }
}
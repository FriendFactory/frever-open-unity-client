using System;
using Common.Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Dropdown
{
    internal sealed class DropdownElement : BaseContextView<DropdownElementModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        [Space] 
        [SerializeField] private Image _background;
        [SerializeField] private Color _evenIndexColor;
        [SerializeField] private Color _oddIndexColor;

        public event Action<int> OnSelected;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        protected override void OnDestroy()
        {
            OnSelected = null;
            CleanUp();
            base.OnDestroy();
        }

        protected override void OnInitialized()
        {
            _label.text = ContextData.Option;
            _background.color = ContextData.DataIndex % 2 == 0 ? _evenIndexColor : _oddIndexColor;
        }

        private void OnClicked()
        {
            OnSelected?.Invoke(ContextData.DataIndex);
        }
    }
}
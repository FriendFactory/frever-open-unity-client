using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel.ColorPicking
{
    internal sealed class FontColorView : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _image;
        [SerializeField] private Image _checkMarkImage;
        private Action<FontColorModel> _onSelected;
        private FontColorModel _model;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(isSelected =>
            {
                if (!isSelected)  return;
                
                _onSelected?.Invoke(_model);
            });
        }

        public void Setup(FontColorModel model, Action<FontColorModel> onSelected)
        {
            _onSelected = onSelected;
            _model = model;
            _image.color = model.Color;
            _checkMarkImage.color = model.CheckmarkColor;
            SetIsSelected(model.IsSelected);
        }

        public void Refresh()
        {
            SetIsSelected(_model.IsSelected);
        }

        private void SetIsSelected(bool isSelected)
        {
            _toggle.SetValueSilently(isSelected);
            _toggle.interactable = !isSelected;//prevent un-selection
        }
    }
}

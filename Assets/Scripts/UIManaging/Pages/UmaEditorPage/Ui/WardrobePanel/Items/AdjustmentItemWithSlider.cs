using Bridge.Models.ClientServer.StartPack.Metadata;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class AdjustmentItemWithSlider : AdjustmentItemBase
    {
        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private float _threshold = 0.01f;
        [SerializeField]
        private TMPro.TextMeshProUGUI _textMeshPro;
        [SerializeField]
        private float _uiMaxValue = 100;
        [SerializeField]
        private float _uiMinValue = -100;

        public override void Setup(UmaAdjustment entity)
        {
            base.Setup(entity);
            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        public override void SetValue(float value)
        {
            _notUserChange = true;
            _slider.value = value;
            _notUserChange = false;
        }

        private void OnValueChanged(float value)
        {
            if (!(_textMeshPro is null))
            {
                var uiValueRange = _uiMaxValue - _uiMinValue;
                var uiValue = (int)((value * uiValueRange) + _uiMinValue);
                _textMeshPro.text = $"{(uiValue > 0 ? "+" : "")}{uiValue}";
            }

            if (_notUserChange) return;

            if (Mathf.Abs(value - _previousValue) < _threshold)
            {
                return;
            }
            
            RaiseAdjustmentChanged(value);
            _previousValue = value;
        }

    }
}

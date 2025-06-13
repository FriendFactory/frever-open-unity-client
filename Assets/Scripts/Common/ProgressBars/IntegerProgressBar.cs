using System;
using Abstract;
using Common.ProgressBars;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Home
{
    public class IntegerProgressBar: BaseContextDataView<IntegerProgressBarModel>, IProgressBar
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _text;
        
        public event Action<float> ValueChanged;
        public float Value
        {
            get => _value;
            set => UpdateProgress(value);
        }

        public float Min => 0;
        public float Max { get; private set; }

        private int _value;
        
        protected override void OnInitialized()
        {
            Max = ContextData.MaxValue;
            _slider.minValue = Min;
            _slider.maxValue = Max;
        }

        private void UpdateProgress(float progress)
        {
            progress = Mathf.Clamp(progress, Min, Max);

            _value = Mathf.FloorToInt(progress);
            
            _slider.value = progress;

            if (_text != null)
            {
                _text.text = $"{_value}/{Mathf.FloorToInt(Max)}";
            }

            ValueChanged?.Invoke(progress);
        }
    }
}
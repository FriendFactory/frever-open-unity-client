using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.ProgressBars
{
    internal sealed class HorizontalProgressBar : ProgressBar
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private RectTransform _gradient;

        private void Start()
        {
            var progressBarSize = GetComponent<RectTransform>().GetSize();
            _gradient.SetSize(progressBarSize);
            _slider.value = 0;
        }

        protected override void OnValueChanged(float value, string textValue)
        {
            _slider.value = value;
            if (_text != null) _text.text = textValue;
        }
    }
}
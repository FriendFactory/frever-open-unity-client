using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.ProgressBars
{
    internal sealed class RadialProgressBar : ProgressBar
    {
        [SerializeField] private Image _progress;
        [SerializeField] private Transform _knob;
        [SerializeField] private TextMeshProUGUI _counter;

        protected override void OnValueChanged(float value, string textValue)
        {
            // Show progress
            _progress.fillAmount = value;
            _counter.text = textValue;
            
            // Move knob
            if (_knob)
            {
                float angle = value * 360f;
                _knob.localEulerAngles = new Vector3(0, 0, -angle);
            }
        }
    }
}
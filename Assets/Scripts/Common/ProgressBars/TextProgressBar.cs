using TMPro;
using UnityEngine;

namespace Common.ProgressBars
{
    internal sealed class TextProgressBar : ProgressBar
    {
        [SerializeField] private TextMeshProUGUI _text;

        protected override void OnValueChanged(float value, string textValue)
        {
            _text.text = textValue;
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public sealed class ToggleTextSwitcher : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private string _textOn;
        [SerializeField] private string _textOff;
        [SerializeField] private TMP_Text _text;
        void Awake()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool isOn)
        {
            _text.text = isOn ? _textOn : _textOff;
        }
    }
}